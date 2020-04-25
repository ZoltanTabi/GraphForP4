import { Component, Input, Output, EventEmitter, HostListener } from '@angular/core';
import { graphviz } from 'd3-graphviz';
import * as d3 from 'd3';
import { Node } from '../models/graph/node';
import { GraphService } from './graph.service';
import { NotificationService } from '../services/notification.service';
import { Edge } from '../models/graph/edge';
import { getSubGraph, getNodeText, getEdgeText } from '../functions/graphConverter';
import { MatBottomSheet } from '@angular/material';
import { BottomSheetTemplateComponent } from '../components/bottom-sheet-template/bottom-sheet-template.component';
import { Data } from '../models/data';
import { Key } from '../models/key';

@Component({
  selector: 'app-graph',
  templateUrl: './graph.component.html',
  providers: [GraphService],
  styleUrls: ['./graph.component.scss']
})
export class GraphComponent {

  @Output() controlFlowGraphClick = new EventEmitter<string>();
  @Output() dataFlowGraphClick = new EventEmitter<string>();
  @Input() type: string;
  @Input() draw: boolean;
  @Input() parentElementId: string;
  @Input() public set initialize(init: boolean) {
    if (!this.init && init) {
      this.init = init;
      this.onInit();
    }
  }

  public loading: boolean;
  public dontTouch: boolean;
  public id: string;
  public graphFord3: string;
  private graphPointer: any;
  private graph: Array<Node>;
  private existNodes: Array<Node>;
  private nextEdges: Array<Edge>;
  private init = false;

  constructor(private graphService: GraphService, private notificationService: NotificationService, public bottomSheet: MatBottomSheet) {
    this.loading = true;
    this.dontTouch = true;
    this.graphFord3 = '';
    this.existNodes = new Array<Node>();
    this.nextEdges = new Array<Edge>();
  }

  onInit(): void {
    this.id = `graph${this.type}`;
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
          while (this.searcNode(i)[0]) {
            ++i;
          }
          this.loading = false;
          this.startGraph(undefined);
          this.dontTouch = false;
          // this.addHandler();
        }
      } else {
        this.loading = false;
        this.notificationService.warning('Üres a gráf!');
      }
    });
  }

  startGraph(i: number) {
    const currentThis = this;
    console.log(this.id);
    const hashTag = '#' + this.id;
    graphviz(hashTag).attributer(this.attributer).transition(this.transition)
      // tslint:disable-next-line: max-line-length
      .renderDot('digraph { graph [id="' + this.id + '" bgcolor="none"] compound=true node [style="filled"] ' + this.graphFord3 + ' } ', function() {
        // document.getElementById('graph0').getElementsByTagName('polygon')[0].setAttribute('fill', 'none');
        if (i === 0) {
          currentThis.graphDrawing(++i, this);
          currentThis.graphPointer = this;
        }
    }).zoom(true);
  }

  graphDrawing(i: number, graph: any) {
    const result = this.searcNode(i);
    const draw = result[0];
    i = result[1];
    if (draw) {
      const currentThis = this;
      // tslint:disable-next-line: max-line-length
      graph.renderDot('digraph { graph [id="' + this.id + '" bgcolor="none"] compound=true node [style="filled"] ' + this.graphFord3 + ' } ', function() {
        // document.getElementById("node1").getElementsByTagName("polygon")[0].setAttribute("fill", "#008000")
        currentThis.graphDrawing(++i, this);
      });
    } else {
      this.dontTouch = false;
      // this.addHandler();
    }
  }

  searcNode(i: number): [boolean, number] {
    let currentGraph = this.graph.filter(x => x.number === i);
    let edgeToNode = new Array<Edge>();
    edgeToNode = edgeToNode.concat(this.nextEdges);
    this.nextEdges = new Array<Edge>();
    // tslint:disable-next-line: no-shadowed-variable
    for (let i = currentGraph.length - 1; i >= 0; --i) {
      this.existNodes.forEach(node => {
        if (currentGraph[i] === node) {
          currentGraph = currentGraph.filter(x => x !== node);
        }
      });
    }

    currentGraph.forEach(node => {
      this.graph.forEach(graphNode => {
        edgeToNode = edgeToNode.concat(graphNode.edges.filter(x => x.child === node.id && this.existNodes.find(y => y.id === x.parent)));
      });
      node.edges.forEach(edge => {
        if (this.existNodes.find(x => x.id === edge.child)) {
          this.nextEdges.push(edge);
        }
      });
    });

    if (this.existNodes.length === this.graph.length && edgeToNode.length === 0) {
      return [false, i];
    }

    if (edgeToNode.length === 0 && currentGraph.length === 0) {
      this.searcNode(++i);
    }

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
        const subGraph = this.graph.filter(x => x.subGraph === node.subGraph);
        subGraph.forEach(subNode => {
          subNode.edges.forEach(edge => {
            if (this.existNodes.find(x => x.id === edge.child)) {
              this.nextEdges.push(edge);
            }
          });
        });
        this.existNodes = this.existNodes.concat(subGraph);
        subGraphNodesId.push(node.subGraph);
      }
    });

    return [true, i];
  }

  attributer(datum: { tag: string; attributes: { width: number; height: number; }; }, index: any, nodes: any) {
    const margin = 20;
    // const hashTag = '#' + this.id;
    // const selection = d3.select(hashTag);
    if (datum.tag === 'svg') {
      // this.parentElementId
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

  onClick(event: any) {
    if (this.dontTouch) {
      return;
    }

    event.path.forEach((element: { id: string; }) => {
      if (this.graph.find(x => x.id === element.id)) {
        if (this.type === Key.ControlFlowGraph) {
          this.controlFlowGraphClickHandler(element.id);
          return;
        } else if (this.type === Key.DataFlowGraph) {
          this.dataFlowGraphClickHandler(element.id);
          return;
        }
      }
    });

    this.reset();
  }

  private controlFlowGraphClickHandler(id: string) {
    const datas: Data[] = [{icon: 'brush', text: 'Csúcs lefolyásának kirajzolása'},
                           {icon: 'add_circle', text: 'Csúcshoz tartozó adatfolyam gráf megjelenítése'}];
    // TODO ellenőrzés, hogy van-e dataFlowGraph csúcsa
    const bottomSheet = this.bottomSheet.open(BottomSheetTemplateComponent, {
      data: datas
    });

    bottomSheet.afterDismissed().subscribe(result => {
      console.log(result);
      if (result) {
        switch (result) {
          case 0:
            this.drawPath(id);
            break;
          case 1:
            this.controlFlowGraphClick.emit(id);
            break;
          default:
            this.notificationService.error('Érvénytelen használat!');
            break;
        }
      }
    });
  }

  private dataFlowGraphClickHandler(id: string) {
    const datas: Data[] = [{icon: 'brush', text: 'Csúcs lefolyásának kirajzolása'},
                           {icon: 'search', text: 'Csúcshoz tartozó vezérlésfolyam gráf csúcs kijelölése'}];

    const bottomSheet = this.bottomSheet.open(BottomSheetTemplateComponent, {
      data: datas
    });

    bottomSheet.afterDismissed().subscribe(result => {
      if (result) {
        switch (result) {
          case 0:
            this.drawPath(id);
            break;
          case 1:
            this.dataFlowGraphClick.emit(id);
            break;
          default:
            this.notificationService.error('Érvénytelen használat!');
            break;
        }
      }
    });
  }

  BFS() {

  }

  drawPath(id: string) {

  }

  selectNode(id: string) {

  }

  reset() {

  }
}
