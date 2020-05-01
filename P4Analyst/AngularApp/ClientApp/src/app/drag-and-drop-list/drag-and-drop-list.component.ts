import { Component, Input, OnInit } from '@angular/core';
import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { Selector } from '../models/selector/selector';
import { Struct } from '../models/variables/struct';
import { Header } from '../models/variables/header';

@Component({
  selector: 'app-drag-and-drop-list',
  templateUrl: './drag-and-drop-list.component.html',
  styleUrls: ['./drag-and-drop-list.component.scss']
})
export class DragAndDropListComponent implements OnInit {

  @Input() public set setSelector(selectors: Array<Selector>) {
    if (selectors && selectors.length > 1) {
      for (let i = 0; i < selectors.length; ++i) {
        const start = this.buildText(selectors[i].startStruct, []).join('; ');
        const end = this.buildText(selectors[i].endStruct, []).join('; ');
        const text = start !== '' && end !== '' ? `${i}. ${start}\n${end}`
            : start !== '' ? `${i}. ${start}` : end !== '' ? `${i}. ${end}` : `${i}. verzió`;

        this.constFromList.push(text);
      }
      this.fromList = this.constFromList;
    }
  }

  private constFromList: Array<string>;
  fromList = new Array<string>();
  toList = new Array<string>();

  constructor() { }

  ngOnInit() {}

  buildText(structs: Array<Struct>, result: string[]): string[] {
    structs.forEach(struct => {
      // tslint:disable-next-line: forin
      for (const key in struct.headers) {
        if ((struct.headers[key] as Header).valid) {
          result.push(key);
        }
      }
      struct.variables.forEach(variable => {
        if (variable.isInitialize) {
          result.push(variable.name);
        }
      });
      // tslint:disable-next-line: forin
      for (const key in struct.structs) {
        this.buildText(struct.structs[key], result);
      }
    });

    return result;
  }

  drop(event: CdkDragDrop<string[]>) {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(event.previousContainer.data, event.container.data, event.previousIndex, event.currentIndex);
      this.fromList = this.constFromList;
    }
  }

  getList(): boolean {
    return this.constFromList && this.constFromList.length > 1;
  }
}
