import { Component, Input, QueryList, ViewChildren, Output, EventEmitter } from '@angular/core';
import { Struct } from '../models/variables/struct';
import { Selector } from '../models/selector/selector';
import { FormControl } from '@angular/forms';
import { StructGroup } from '../models/selector/structGroup';
import { StructViewModel } from '../models/selector/structViewModel';
import { Header } from '../models/variables/header';
import { MatExpansionPanel } from '@angular/material';
import { Copy } from '../functions/copy';

@Component({
  selector: 'app-selector',
  templateUrl: './selector.component.html',
  styleUrls: ['./selector.component.scss']
})
export class SelectorComponent {

  @ViewChildren(MatExpansionPanel) panels: QueryList<MatExpansionPanel>;

  @Output() change = new EventEmitter();
  @Input() public set structs(structs: Array<Struct>) {
    if (!this.originalStructs && structs) {
      this.originalStructs = structs;
      this.onInit();
    }
  }
  public selectors = new Array<Selector>();
  public viewModels = new Array<StructGroup>();
  private originalStructs: Array<Struct>;
  public colspan: number;


  constructor() {
    this.colspan = (window.innerWidth <= 700) ? 2 : 1;
  }

  onInit() {
    this.originalStructs.forEach(struct => {
      this.viewModelBuild(struct, struct.name);
    });
    this.selectors.push({
      id: 1,
      startStruct: Copy<Array<Struct>>(this.originalStructs),
      endStruct: Copy<Array<Struct>>(this.originalStructs),
      startControl: new FormControl(),
      endControl: new FormControl()
    });
    console.log(this.viewModels);
  }

  add() {
    this.selectors.push({
      id: this.selectors.length + 1,
      startStruct: Copy<Array<Struct>>(this.originalStructs),
      endStruct: Copy<Array<Struct>>(this.originalStructs),
      startControl: new FormControl(),
      endControl: new FormControl()
    });
    this.change.emit();
    this.panels.forEach(panel => {
      panel.close();
    });
    this.delay().then(() => {
      this.panels.last.open();
    });
  }

  delay = () => {
    return new Promise<void>((resolve) => {
      const time = setInterval(() => {
          clearInterval(time);
          resolve();
      }, 400);
    });
  }

  cancel(id: number) {
    this.selectors.splice(id - 1, 1);
    this.selectors.forEach(x => {
      if (x.id > id) {
        --x.id;
      }
    });
    this.change.emit();
  }

  onResize() {
    this.colspan = (window.innerWidth <= 700) ? 2 : 1;
  }

  viewModelBuild(struct: Struct, name: string) {
    const newViewModel: StructGroup = {name: name, views: new Array<StructViewModel>()};
    // tslint:disable-next-line: forin
    for (const key in struct.headers) {
      newViewModel.views.push({value: `${newViewModel.name} - ${key}`, viewValue: `${struct.headers[key].name} - fejléc`});
    }
    /*struct.variables.forEach(variable => {
      const variableName = variable.name;
      newViewModel.views.push({value: `${newViewModel.name} - ${variableName}`, viewValue: `${variableName} - változó`});
    });*/
    this.viewModels.push(newViewModel);
    // tslint:disable-next-line: forin
    for (const key in struct.structs) {
      this.viewModelBuild(struct.structs[key], `${name} - ${key}`);
    }
  }

  onStartSelect(id: number, event: any) {
    const startStruct = this.selectors.find(x => x.id === id).startStruct;
    const path = (event.source.value as string).split(' - ');
    const newValue = event.source.selected as boolean;
    this.onSelect(startStruct, path, newValue);
  }

  onEndSelect(id: number, event: any) {
    const endStruct = this.selectors.find(x => x.id === id).endStruct;
    const path = (event.source.value as string).split(' - ');
    const newValue = event.source.selected as boolean;
    this.onSelect(endStruct, path, newValue);
  }

  onSelect(structs: Array<Struct>, path: string[], newValue: boolean) {
    let struct: Struct;

    for (let i = 0; i < path.length; ++i) {
      if ( i === path.length - 1) {
        const varible = struct.variables.find(x => x.name === path[i]);
        if (varible) {
          varible.isInitialize = newValue;
        } else {
          const header = struct.headers[path[i]] as Header;
          header.valid = newValue;
          header.variables.forEach(x => {
            x.isInitialize = newValue;
          });
        }
      } else {
        if (struct) {
          struct = struct.structs[path[i]];
        } else {
          struct = structs.find(x => x.name === path[0]);
        }
      }
    }
    this.change.emit();
  }
}
