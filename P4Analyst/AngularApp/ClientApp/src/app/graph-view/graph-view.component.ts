import { Component, AfterViewInit, ViewChildren, QueryList, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { ResizedEvent } from 'angular-resize-event';
import { Key } from '../models/key';
import { GraphComponent } from '../graph/graph.component';
import { MatAnimatedIconComponent } from '../components/mat-animated-icon/mat-animated-icon.component';
import { SessionStorageService } from 'ngx-store';
import { TabContent } from '../models/tabContent';
import { Node } from '../models/graph/node';
import { SizeAttribute } from '../models/sizeAttribute';

@Component({
  selector: 'app-graph-view',
  templateUrl: './graph-view.component.html',
  styleUrls: ['./graph-view.component.scss']
})
export class GraphViewComponent {

  @ViewChildren(GraphComponent) graphs: QueryList<GraphComponent>;
  @ViewChildren(MatAnimatedIconComponent) icons: QueryList<MatAnimatedIconComponent>;

  tabs: TabContent[];
  selected = new FormControl(0);
  dataflowgraphIsActiveTab = false;
  keys = [Key.ControlFlowGraph, Key.DataFlowGraph];
  currentSize: SizeAttribute;
  buttonsDisabled = [true, true];

  constructor(private sessionStorageService: SessionStorageService) {
    this.tabs = new Array<TabContent>();
  }

  onTabChange(selectedTab: number) {
    if (selectedTab === 1) {
      this.dataflowgraphIsActiveTab = true;
    } else if (this.dataflowgraphIsActiveTab) {
      this.graphs.find(x => x.type === Key.DataFlowGraph).stopBFS();
    }
    if (selectedTab !== 0) {
      this.graphs.first.stopBFS();
    }
    this.buttonsDisabled = [true, true];
    this.selected.setValue(selectedTab);
    this.resizeActiveGraph(selectedTab);
  }

  addDataFlowGraph(id: string) {
    const tab = this.tabs.find(x => x.id === id);
    if (tab) {
      this.selected.setValue(this.tabs.indexOf(tab) + 2);
    } else {
      let subGraph = this.sessionStorageService.get(id) as Array<Node>;
      if (!subGraph) {
        const dataFlowGraph = this.sessionStorageService.get(Key.DataFlowGraph) as Array<Node>;
        subGraph = dataFlowGraph.filter(x => x.parentId === id);
        subGraph.forEach(node => {
          for (let i = node.edges.length - 1; i >= 0; --i) {
            if (dataFlowGraph.find(x => x.id === node.edges[i].child).parentId !== id) {
              node.edges.splice(i, 1);
            }
          }
        });
        this.sessionStorageService.set(id, subGraph);
      }
      let name = (this.sessionStorageService.get(Key.ControlFlowGraph) as Array<Node>).find(x => x.id === id).text;
      name = (name && name.length < 20) ? name : name.substr(0, 15) + '...';
      this.tabs.push({graph: subGraph, id: id, name: name});
      this.selected.setValue(this.tabs.length + 1);
    }
  }

  removeTab(index: number) {
    this.tabs.splice(index, 1);
  }

  onResized(event: ResizedEvent) {
    this.currentSize = {width: event.newWidth, height: event.newHeight};
    this.resizeActiveGraph(this.selected.value);
  }

  resizeActiveGraph(selectTab: number) {
    if (selectTab === 0) {
      const graph = this.graphs.find(x => x.type === Key.ControlFlowGraph);
      if (!graph.dontTouch) {
        this.buttonsDisabled[0] = false;
      }
      graph.resize(this.currentSize);
    } else if (selectTab === 1) {
      const graph = this.graphs.find(x => x.type === Key.DataFlowGraph);
      if (!graph.dontTouch) {
        this.buttonsDisabled[1] = false;
      }
      graph.resize(this.currentSize);
    } else {
      this.graphs.find(x => x.type === this.tabs[selectTab - 2].id).resize(this.currentSize);
    }
  }

  callControlFlowGraphSelect(id: string) {
    this.selected.setValue(0);
    this.graphs.first.selectNode(id);
  }

  setIconToogle(i: number) {
    if (i === 0) {
      this.icons.first.toggle();
    } else if (i === 1) {
      this.icons.last.toggle();
    }
  }

}
