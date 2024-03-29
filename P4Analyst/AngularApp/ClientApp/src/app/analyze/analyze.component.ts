import { Component, OnInit, ViewChildren, ViewChild } from '@angular/core';
import { AnalyzeService } from './analyze.service';
import { Struct } from '../models/variables/struct';
import { MatStepper, MatButtonToggleGroup, MatDialog } from '@angular/material';
import { MAT_STEPPER_GLOBAL_OPTIONS } from '@angular/cdk/stepper';
import { SessionStorageService } from 'ngx-store';
import { Key } from '../models/key';
import { SelectorComponent } from '../selector/selector.component';
import { DragAndDropListComponent } from '../drag-and-drop-list/drag-and-drop-list.component';
import { AnalyzeData } from '../models/variables/analyzeData';
import { ChartsDisplayComponent } from '../charts-display/charts-display.component';
import { NameDialogComponent } from './name-dialog/name-dialog.component';
import { CalculatedData } from '../models/calculate/calculatedData';
import { NotificationService } from '../services/notification.service';
import { FileService } from '../services/file.service';

@Component({
  selector: 'app-analyze',
  templateUrl: './analyze.component.html',
  providers: [AnalyzeService, {
      // tslint:disable-next-line: deprecation
      provide: MAT_STEPPER_GLOBAL_OPTIONS, useValue: {displayDefaultIndicatorType: false}
    }],
  styleUrls: ['./analyze.component.scss']
})
export class AnalyzeComponent implements OnInit {

  public originalStructs: Array<Struct>;
  @ViewChild('stepper', {static: true}) stepper: MatStepper;
  @ViewChild('buttonGroup',  {static: true}) buttonGroup: MatButtonToggleGroup;
  @ViewChild('selector', {static: true}) selector: SelectorComponent;
  @ViewChild('dragAndDropList', {static: true}) dragAndDropList: DragAndDropListComponent;
  @ViewChild('chartsDisplay', {static: true}) chartsDisplay: ChartsDisplayComponent;

  constructor(private analyzeService: AnalyzeService, private sessionStorageService: SessionStorageService, public dialog: MatDialog,
    private notificationService: NotificationService, private fileService: FileService) { }

  ngOnInit() {
    this.originalStructs = this.sessionStorageService.get(Key.Structs) as Array<Struct>;
    if (!this.originalStructs) {
      this.analyzeService.getVariables().subscribe(result => {
        console.log(result);
        this.originalStructs = result;
        this.sessionStorageService.set(Key.Structs, this.originalStructs);
      });
    }
  }

  onPut() {
    const message = new Array<AnalyzeData>();
    const toList = this.dragAndDropList.toList;
    if (toList && toList.length > 0) {
      for (let i = 0; i < toList.length; ++i) {
        const selector = this.selector.selectors.find(x => x.id === toList[i].id);
        message.push({id: selector.id, startState: selector.startStruct, endState: selector.endStruct});
      }
    } else {
      this.selector.selectors.forEach(x => {
        message.push({id: x.id, startState: x.startStruct, endState: x.endStruct});
      });
    }

    this.analyzeService.putStructs(message).subscribe((result: any) => {
      console.log(result);
      this.chartsDisplay.calculatedDataSetter = result;
      const question = this.sessionStorageService.get(Key.Question) as boolean;
      if (!question) {
        const fileData = (result as CalculatedData).file;
        const dialogRef = this.dialog.open(NameDialogComponent, {
          height: (window.innerWidth >= 600) ? '35%' : '100%',
          width: (window.innerWidth >= 600) ? '40%' : '100%',
          data: { name: fileData.name },
          disableClose: true
        });

        dialogRef.afterClosed().subscribe(dialogResult => {
          if (dialogResult) {
            fileData.name = dialogResult;
            this.fileService.uploadFile(fileData).subscribe(() => {
              this.notificationService.success('Sikeres feltöltés!');
            });
          }
          this.sessionStorageService.set(Key.Question, true);
        });
      }
    });
  }

  onButtonChange(value: string) {
    console.log(value);
    if (value === 'before') {
      --this.stepper.selectedIndex;
      if (this.selector.selectors.length === 1 && this.stepper.selectedIndex === 1) {
        this.stepper.selectedIndex = 0;
      }
    } else if (value === 'next') {
      ++this.stepper.selectedIndex;
      if (this.selector.selectors.length === 1 && this.stepper.selectedIndex === 1) {
        this.stepper.selectedIndex = 2;
      }
    } else {
      this.stepper.selectedIndex = +value;
    }

    if (this.stepper.selectedIndex === 2) {
      this.onPut();
    }
    // tslint:disable-next-line: triple-equals
    this.buttonGroup._buttonToggles.find(x => x.value == value).checked = false;
    // tslint:disable-next-line: triple-equals
    this.buttonGroup._buttonToggles.find(x => x.value == this.stepper.selectedIndex).checked = true;
  }

  onSelectorChange() {
    this.dragAndDropList.setSelector(this.selector.selectors);
  }
}
