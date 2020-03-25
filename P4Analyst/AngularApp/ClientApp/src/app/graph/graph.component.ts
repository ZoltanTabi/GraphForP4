import { Component, OnInit, Input } from '@angular/core';
import { graphviz } from 'd3-graphviz';
import * as d3 from 'd3';
import { Node } from '../models/node';
import { GraphService } from './graph.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-graph',
  templateUrl: './graph.component.html',
  providers: [GraphService],
  styleUrls: ['./graph.component.scss']
})
export class GraphComponent implements OnInit {

  @Input() type: string;
  @Input() draw: boolean;
  private number: number;
  private list: Array<string>;
  private graph: Array<Node>;

  constructor(private graphService: GraphService, private router: Router) {
    this.number = 0;
    this.list = ['start [fontcolor="#008000" shape="diamond"]' ,
      'node [style="filled"] start [fillcolor="#008000" shape="diamond"] start -> b',
      'node [style="filled"] start [fillcolor="#008000" shape="diamond"] start -> b; start -> c',
      'node [style="filled"] start [fillcolor="#008000" shape="diamond"] start -> b; start -> c; b -> d; c -> d'];
  }

  ngOnInit(): void {
    this.graphService
    .getGraph(this.type)
    .subscribe(result => {
      this.graph = result;
      console.log(this.graph);
    });

    const currentThis = this;
    graphviz('#graph').transition(() => d3.transition().duration(500))
      .renderDot('digraph { ' + this.list[this.number] + ' } ', function() {
        currentThis.graphDrawing(++currentThis.number, this);
      });
  }

  graphDrawing(i: number, graph: any) {
    if (i !== this.list.length) {
      const currentThis = this;
      graph.renderDot('digraph { ' + currentThis.list[i] + ' } ', function() {
        currentThis.graphDrawing(++i, this);
      });
    } else {
      console.log(graph);

      const nodes = d3.selectAll('.node');
      nodes.on('click', this.fieldClickHandler);
    }
  }

  private fieldClickHandler() {
    console.log(d3.event.toElement.__data__.parent.key);
  }
}
