import { Component, OnInit } from '@angular/core';
import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { Selector } from '../models/selector/selector';
import { Struct } from '../models/variables/struct';
import { Header } from '../models/variables/header';
import { Copy } from '../functions/copy';
import { DragItem } from '../models/dragItem';

@Component({
  selector: 'app-drag-and-drop-list',
  templateUrl: './drag-and-drop-list.component.html',
  styleUrls: ['./drag-and-drop-list.component.scss']
})
export class DragAndDropListComponent implements OnInit {

  private constFromList: Array<DragItem>;
  fromList = new Array<DragItem>();
  toList = new Array<DragItem>();

  constructor() { }

  ngOnInit() {}

  setSelector(selectors: Array<Selector>) {
    if (selectors && selectors.length > 1) {
      this.constFromList = new Array<DragItem>();
      for (let i = 0; i < selectors.length; ++i) {
        const start = this.buildText(selectors[i].startStruct, []).join('; ');
        const end = this.buildText(selectors[i].endStruct, []).join('; ');
        const text = start !== '' && end !== '' ? `${start} - ${end}`
            : start !== '' ? start : end !== '' ? end : `${i + 1}. verzió`;

        this.constFromList.push({id: i + 1, text: text});
      }
      this.fromList = Copy<Array<DragItem>>(this.constFromList);
      this.toList = new Array<DragItem>();
    }
  }

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
      const structArray = new Array<Struct>();
      // tslint:disable-next-line: forin
      for (const key in struct.structs) {
        structArray.push(struct.structs[key]);
      }
      this.buildText(structArray, result);
    });

    return result;
  }

  drop(event: CdkDragDrop<string[]>) {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(event.previousContainer.data, event.container.data, event.previousIndex, event.currentIndex);
      this.fromList = Copy<Array<DragItem>>(this.constFromList);
    }
  }

  getList(): boolean {
    return this.constFromList && this.constFromList.length > 1;
  }
}
