import { Component, OnInit, Input } from '@angular/core';
import { graphviz } from 'd3-graphviz';
import * as d3 from 'd3';
import { Node } from '../models/node';
import { GraphService } from './graph.service';
import { NotificationService } from '../services/notification.service';
import { Edge } from '../models/edge';
import { getSubGraph, getNodeText, getEdgeText } from '../functions/graphConverter';

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
  public graphFord3: string;
  private graphPointer: any;
  private graph: Array<Node>;
  private existNodes: Array<Node>;
  private currentNodes: Array<Node>;

  constructor(private graphService: GraphService, private notificationService: NotificationService) {
    this.loading = true;
    this.dontTouch = true;
    this.graphFord3 = '';
    this.existNodes = new Array<Node>();
    this.currentNodes = new Array<Node>();
  }

  ngOnInit(): void {
    this.graphService
    .getGraph(this.type)
    .subscribe( result => {
      console.log(result);
      this.graph = result;

      if (this.graph.length > 0) {
        this.loading = false;
        if (this.draw) {
          this.searcNode(0);
          this.startGraph(0);
        } else {
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
        this.notificationService.warning('Üres a gráf!');
      }
    });
  }

  startGraph(i: number) {
    const currentThis = this;
    graphviz('#graph').attributer(this.attributer).transition(this.transition)
      .renderDot('digraph { graph [bgcolor="none"] compound=true node [style="filled"] ' + this.graphFord3 + ' } ', function() {
        // document.getElementById('graph0').getElementsByTagName('polygon')[0].setAttribute('fill', 'none');
        if (i === 0) {
          currentThis.graphDrawing(++i, this);
          currentThis.graphPointer = this;
        }
    }).zoom(true);
  }

  graphDrawing(i: number, graph: any) {
    const draw = this.searcNode(i);
    if (draw) {
      const currentThis = this;
      graph.renderDot('digraph { graph [bgcolor="none"] node [style="filled"] ' + this.graphFord3 + ' } ', function() {
        // document.getElementById("node1").getElementsByTagName("polygon")[0].setAttribute("fill", "#008000")
        currentThis.graphDrawing(++i, this);
      });
    } else {
      this.addHandler();
    }
  }

  searcNode(i: number): boolean {
    let currentGraph = this.graph.filter(x => x.number === i);
    // tslint:disable-next-line: no-shadowed-variable
    for (let i = currentGraph.length - 1; i >= 0; --i) {
      this.existNodes.forEach(node => {
        if (currentGraph[i] === node) {
          currentGraph = currentGraph.filter(x => x !== node);
        }
      });
    }

    if (currentGraph.length === 0) {
      return false;
    }
    console.log(currentGraph);

    let edgeToNode = new Array<Edge>();
    currentGraph.forEach(node => {
      this.graph.forEach(graphNode => {
        edgeToNode = edgeToNode.concat(graphNode.edges.filter(x => x.child === node.id));
      });
    });

    edgeToNode.forEach(edge => {
      this.graphFord3 += getEdgeText(edge, this.graph);
    });

    currentGraph.filter(x => x.subGraph === '').forEach(node => {
      this.graphFord3 += getNodeText(node);
      this.existNodes.push(node);
    });

    const subGraphNodesId = new Array<string>();
    currentGraph.filter(x => x.subGraph !== '').forEach(node => {
      if (subGraphNodesId.filter(x => x === node.subGraph).length === 0) {
        this.graphFord3 += getSubGraph(node.subGraph, this.graph);
        this.existNodes = this.existNodes.concat(this.graph.filter(x => x.subGraph === node.subGraph));
        subGraphNodesId.push(node.subGraph);
      }
    });

    return true;
  }

  addHandler() {
    const nodes = d3.selectAll('.node');
    nodes.on('click', this.fieldClickHandler);
    this.dontTouch = false;
  }

  private fieldClickHandler() {
    console.log(d3.event.toElement.__data__.parent.key);
  }

  attributer(datum: { tag: string; attributes: { width: number; height: number; }; }, index: any, nodes: any) {
    const margin = 20;
    const selection = d3.select('#graph');
    if (datum.tag === 'svg') {
        const width = document.getElementById('mat-sidenav-content').offsetWidth;
        const height = document.getElementById('mat-sidenav-content').offsetHeight;
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
