import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { ResizedEvent } from 'angular-resize-event';
import { Key } from '../models/key';

@Component({
  selector: 'app-graph-view',
  templateUrl: './graph-view.component.html',
  styleUrls: ['./graph-view.component.scss']
})
export class GraphViewComponent implements OnInit {

  tabs = [];
  selected = new FormControl(0);
  dataflowgraphIsActiveTab = false;
  keys = [Key.ControlFlowGraph, Key.DataFlowGraph];

  constructor() { }

  ngOnInit() {
  }

  onTabChange(selectedTab: number) {
    if (selectedTab === 1) {
      this.dataflowgraphIsActiveTab = true;
    }
    this.selected.setValue(selectedTab);
  }

  addDataFlowGraph(tabName: string) {
    const tab = { tabName: tabName };
    this.tabs.push(tab);

    this.selected.setValue(this.tabs.length - 1);
  }

  removeTab(index: number) {
    this.tabs.splice(index, 1);
  }

  onResized(event: ResizedEvent) {
  }

}
