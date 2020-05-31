import { Component, OnInit, Input, ViewChildren, QueryList } from '@angular/core';
import { Node } from 'src/app/models/graph/node';
import { ResizedEvent } from 'angular-resize-event';
import { SizeAttribute } from 'src/app/models/sizeAttribute';
import { GraphComponent } from 'src/app/graph/graph.component';

@Component({
  selector: 'app-graph-carousel',
  templateUrl: './graph-carousel.component.html',
  styleUrls: ['./graph-carousel.component.scss'],
})
export class GraphCarouselComponent implements OnInit {
  @ViewChildren(GraphComponent) graphQuery: QueryList<GraphComponent>;

  @Input() public set graphsSetter(graphs: Array<Node[]>) {
    if (graphs && graphs.length > 0 && this.graphs !== graphs) {
      this.graphs = graphs;
    }
  }
  @Input() id: string;

  current = 0;
  public graphs: Array<Node[]>;

  constructor() { }

  ngOnInit() { }

  next() {
    this.current = (this.current + 1) % this.graphs.length;
  }

  prev() {
    this.current = (this.current - 1 + this.graphs.length) % this.graphs.length;
  }

  onResized(event: ResizedEvent) {
    const currentSize: SizeAttribute = {width: event.newWidth, height: event.newHeight};
    const graph = this.graphQuery.find(x => x.type === this.id + this.current);
    if (graph) {
      graph.resize(currentSize);
    }
  }

}
