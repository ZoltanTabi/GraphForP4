import { Component, OnInit, Input } from '@angular/core';
import { graphviz } from 'd3-graphviz';
import * as d3 from 'd3';
import { Node } from '../models/node';
import { GraphService } from './graph.service';
import { Router } from '@angular/router';
import { error } from '@angular/compiler/src/util';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-graph',
  templateUrl: './graph.component.html',
  providers: [GraphService],
  styleUrls: ['./graph.component.scss']
})
export class GraphComponent implements OnInit {

  @Input() type: string;
  @Input() draw: boolean;

  public loading: boolean;
  public dontTouch: boolean;
  public tuple: [string, string];
  private graph: Array<Node>;
  private currentEdges: Array<[[string, string], string]>;

  constructor(private graphService: GraphService, private router: Router, private snackBar: MatSnackBar) {
    this.loading = true;
    this.tuple = ['', ''];
    this.dontTouch = true;
    this.currentEdges = new Array<[[string, string], string]>();
  }

  ngOnInit(): void {
    this.graphService
    .getGraph(this.type)
    .subscribe( result => {
      this.graph = this.deserialize(result);

      if (this.graph.length > 0) {
        this.loading = false;
        if (this.draw) {
          this.searcNode(0);
          this.startGraph(0);
        } else {
          // this.graph.forEach(node => {
          //  this.addTuple(node);
          // });
          let i = 0;
          while (this.searcNode(i)) {
            ++i;
          }
          this.loading = false;
          this.startGraph(undefined);
          this.addHandler();
        }
      } else {
        this.loading = false;
        this.snackBar.open('Üres a gráf', 'Hiba!', {
          duration: 50000,
          panelClass: ['error-snackbar']
        });
      }
    }, () => {
        console.log(error);
    }, () => {

    });
  }

  startGraph(i: number) {
    // tslint:disable-next-line: no-shadowed-variable
    const currentThis = this;
    graphviz('#graph').attributer(this.attributer).transition(this.transition)
      .renderDot('digraph { ' + this.tuple[0] + ' ' + this.tuple[1] + ' } ', function() {
        if (i === 0) {
          currentThis.graphDrawing(++i, this);
        }
    }).zoom(true);
  }

  graphDrawing(i: number, graph: any) {
    const draw = this.searcNode(i);
    if (draw || this.currentEdges.length !== 0) {
      const currentThis = this;
      graph.renderDot('digraph { ' + this.tuple[0] + ' ' + this.tuple[1] + ' } ', function() {
        currentThis.graphDrawing(++i, this);
      });
    } else {
      this.addHandler();
    }
  }

  searcNode(i: number): boolean {
    let draw = false;

    for (let edge = this.currentEdges.pop(); edge; edge = this.currentEdges.pop()) {
      draw = true;
      this.tuple[1] += edge[0][0] + ' -> ' + edge[0][1] + ' ' + edge[1] + ' ';
    }

    this.graph.forEach(item => {
      if (item.number === i) {
        draw = true;
        // this.addTuple(item);
        this.tuple[0] += item.node[0] + ' ' + item.node[1] + ' ';
        item.edges.forEach(edge => {
          this.currentEdges.push(edge);
        });
      }
    });

    return draw;
  }

  // addTuple(node: Node) {
  //  this.tuple[0] += node.node[0] + ' ' + node.node[1] + ' ';
  //  this.graph.forEach(item => {
  //    item.edges.forEach(edge => {
  //      if (edge[0][1] === node.node[0] && item.number <= node.number) {
  //        this.tuple[1] += edge[0][0] + ' -> ' + edge[0][1] + ' ' + edge[1] + ' ';
  //      }
  //    });
  //  });
  //  node.edges.forEach(edge => {
  //    this.graph.forEach(item => {
  //      if (item.node[0] === edge[0][1] && item.number < node.number) {
  //        this.tuple[1] += edge[0][0] + ' -> ' + edge[0][1] + ' ' + edge[1] + ' ';
  //      }
  //    });
  //  });
  // }

  // otherEdge(i: number): boolean {
  //  let find = false;
  //  this.graph.forEach(item => {
  //    if (item.number === i && item.edges.length > 0) {
  //      find = true;
  //      item.edges.forEach(edge => {
  //        this.tuple[1] += edge[0][0] + ' -> ' + edge[0][1] + ' ' + edge[1] + ' ';
  //      });
  //    }
  //  });
  //  return find;
  // }

  addHandler() {
    const nodes = d3.selectAll('.node');
    nodes.on('click', this.fieldClickHandler);
    this.dontTouch = false;
  }

  private fieldClickHandler() {
    console.log(d3.event.toElement.__data__.parent.key);
  }

  private deserialize (result: Node[]): Node[] {
    // tslint:disable-next-line: prefer-const
    let newGraph = new Array<Node>();

    result.forEach(node => {
      const newNumber = node.number;
      const newNodeItem: [string, string] = [node.node['item1'], node.node['item2']];
      const newEdges = new Array<[[string, string], string]>();
      node.edges.forEach(edge => {
        newEdges.push([[edge['item1']['item1'], edge['item1']['item2']], edge['item2']]);
      });
      const newNode: Node = {number: newNumber, node: newNodeItem, edges: newEdges};
      newGraph.push(newNode);
    });

    return newGraph;
  }

  attributer(datum: { tag: string; attributes: { width: number; height: number; }; }, index: any, nodes: any) {
    const margin = 20;
    const selection = d3.select('#graph');
    if (datum.tag === 'svg') {
        const width = document.getElementById('mat-sidenav-content').offsetWidth;
        const height = document.getElementById('mat-sidenav-content').offsetHeight;
        // const width = window.innerWidth;
        // const height = window.innerHeight;
        datum.attributes.width = width - margin;
        datum.attributes.height = height - margin;
    }
  }

  transition() {
    return d3.transition('graphviz')
        .ease(d3.easeLinear)
        .delay(40)
        .duration(2000);
  }
}
