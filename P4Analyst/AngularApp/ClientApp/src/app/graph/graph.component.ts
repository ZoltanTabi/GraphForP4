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
import { Queue } from 'queue-typescript';
import { Data } from '../models/data';
import { Key } from '../models/key';
import { Color } from '../models/color';
import { BottomSheetYesOrNoComponent } from '../components/bottom-sheet-yes-or-no/bottom-sheet-yes-or-no.component';
import { SessionStorageService } from 'ngx-store';
import { SizeAttribute } from '../models/sizeAttribute';
import { Copy } from '../functions/copy';

@Component({
  selector: 'app-graph',
  templateUrl: './graph.component.html',
  providers: [GraphService],
  styleUrls: ['./graph.component.scss']
})
export class GraphComponent {

  @Output() controlFlowGraphClick = new EventEmitter<string>();
  @Output() dataFlowGraphClick = new EventEmitter<string>();
  @Output() BFSEnd = new EventEmitter();
  @Output() drawEnd = new EventEmitter();
  @Input() type: string;
  @Input() draw: boolean;
  @Input() inputGraph: Array<Node>;
  @Input() parentElementId: string;
  @Input() zoom = false;
  @Input() skipInfoMessage = false;
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
  private graph: Array<Node>;
  private existNodes: Array<Node>;
  private nextEdges: Array<Edge>;
  private init = false;

  private colorNodes: Array<Node>;
  private BFSQueue: Queue<Node>;
  private BFSIsRun = false;
  private oldSize: SizeAttribute;
  private timer: any;

  constructor(private graphService: GraphService, private notificationService: NotificationService,
    public bottomSheet: MatBottomSheet, private sessionStorageService: SessionStorageService) {
    this.loading = true;
    this.dontTouch = true;
    this.graphFord3 = '';
    this.existNodes = new Array<Node>();
    this.nextEdges = new Array<Edge>();
    this.BFSQueue = new Queue<Node>();
    this.colorNodes = new Array<Node>();
  }

  // Gráf megkapása, leszedjük szerver oldalró, vagy session-ból vagy inputból kapjuk meg
  // Mivel a Mat-Tab content csak a aktív tab esetén jelenik meg a HTML kódban, ezért kell az async művelet és a delay
  getGraph = () => {
    return new Promise<void>((resolve, reject) => {
      if (this.inputGraph) {
        this.graph = this.inputGraph;
        const time = setInterval(() => {
            clearInterval(time);
            resolve();
        }, 1000);
      } else {
        const sessionGraph = this.sessionStorageService.get(this.type) as Array<Node>;
        if (sessionGraph) {
          this.graph = sessionGraph;
          const time = setInterval(() => {
              clearInterval(time);
              resolve();
          }, 1000);
        } else {
            reject('CallServer');
        }
      }
    });
  }

  onInit() {
    this.id = `graph${this.type}`;
    if (!this.skipInfoMessage) {
      this.notificationService.info('Kérjük várja meg, amíg betöltődik!');
    }
    this.getGraph()
      .then(() => { this.afterOnInit(); })
      .catch((reason) => {
        if (reason === 'CallServer') {
          this.graphService
          .getGraph(this.type)
          .subscribe( result => {
            this.graph = result;
            this.sessionStorageService.set(this.type, this.graph);
            this.afterOnInit();
          });
        } else {
          if (this.type === Key.ControlFlowGraph || this.type === Key.DataFlowGraph) {
            this.notificationService.error('Hiba történt! Frissítsen rá az oldalra!');
          } else {
            this.notificationService.error('Zárja be a tabot és próbálja újra!');
          }
        }
      });
  }

  afterOnInit() {
    console.log(this.graph);

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
        this.drawEnd.emit();
      }
    } else {
      this.loading = false;
      this.notificationService.warning('Üres a gráf!');
    }
  }

  // d3-graphviz gráf kirajzolás
  startGraph(i: number) {
    const currentThis = this;
    const hashTag = '#' + this.id;
    graphviz(hashTag).attributer(this.attributer).transition(this.transition)
      // tslint:disable-next-line: max-line-length
      .renderDot('digraph { graph [id="' + this.id + '" bgcolor="none" tooltip="Helyreállítésrt klikkelj!"] compound=true node [style="filled"] ' + this.graphFord3 + ' } ', function() {
        if (i === 0) {
          currentThis.graphDrawing(++i, this);
        }
    }).zoom(this.zoom || window.innerWidth >= 600);
  }

  graphDrawing(i: number, graph: any) {
    const result = this.searcNode(i);
    const draw = result[0];
    i = result[1];
    if (draw) {
      const currentThis = this;
      // tslint:disable-next-line: max-line-length
      graph.renderDot('digraph { graph [id="' + this.id + '" bgcolor="none" tooltip="Helyreállítésrt klikkelj!"] compound=true node [style="filled"] ' + this.graphFord3 + ' } ', function() {
        currentThis.graphDrawing(++i, this);
      });
    } else {
      this.dontTouch = false;
      this.drawEnd.emit();
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
        if (this.existNodes.find(x => x.id === edge.child) || currentGraph.find(x => x.id === edge.child)) {
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

  attributer(datum: { tag: string; attributes: { width: number; height: number; }; }) {
    const margin = 20;
    if (datum.tag === 'svg') {
      // this.parentElementId
      const width = document.getElementById('mat-sidenav-content').offsetWidth;
      const height = document.getElementById('mat-sidenav-content').offsetHeight - 48;
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

  resize = (size: SizeAttribute) => {
    return new Promise<Node>(() => {
      // tslint:disable-next-line:max-line-length
      if (window.innerWidth >= 600 && size && size.width >= 0 && size.height >= 60 && !this.loading && (!this.oldSize || size !== this.oldSize)) {
        this.oldSize = size;
        let delayTime = 0;
        const time = setInterval(() => {
          if (delayTime !== 0) {
            ++delayTime;
          } else {
            clearInterval(time);
            const attributes = document.getElementById(this.id).children[0];
            attributes.setAttribute('width', size.width.toString());
            attributes.setAttribute('height', (size.height - 60).toString());
          }
        }, 1000);
      }
    });
  }

  // Click események kezelése
  onClick(event: any) {
    if (this.dontTouch) {
      return;
    }

    let otherBottomSheet = false;
    event.path.forEach((element: { id: string; }) => {
      if (this.graph.find(x => x.id === element.id)) {
        if (this.type === Key.ControlFlowGraph) {
          otherBottomSheet = true;
          this.controlFlowGraphClickHandler(element.id);
        } else if (this.type === Key.DataFlowGraph) {
          otherBottomSheet = true;
          this.dataFlowGraphClickHandler(element.id);
        }
      }
    });

    if ((this.type === Key.ControlFlowGraph || this.type === Key.DataFlowGraph) && this.colorNodes.length > 0 && !otherBottomSheet) {
      const data: Data = { icon: '', text: 'Biztosan helyrállítja a gráf színezést?' };
      const bottomSheet = this.bottomSheet.open(BottomSheetYesOrNoComponent, {
        data: data,
        panelClass: 'my-theme'
      });
      bottomSheet.afterDismissed().subscribe(result => {
        if (result) {
          this.reset();
        }
      });
    }
  }

  private controlFlowGraphClickHandler(id: string) {
    const datas: Data[] = [{icon: 'brush', text: 'Csúcs lefolyásának kirajzolása'}];

    const sessionDataFlowGraph = this.sessionStorageService.get(Key.DataFlowGraph) as Array<Node>;
    if (sessionDataFlowGraph && sessionDataFlowGraph.find(x => x.parentId === id)) {
      datas.push({icon: 'add_circle', text: 'Csúcshoz tartozó adatfolyam gráf megjelenítése'});
    }

    const bottomSheet = this.bottomSheet.open(BottomSheetTemplateComponent, {
      data: datas
    });

    bottomSheet.afterDismissed().subscribe(result => {
      if (result === 0) {
        this.drawPath(id);
      } else if (result === 1) {
        this.controlFlowGraphClick.emit(id);
      }
    });
  }

  private dataFlowGraphClickHandler(id: string) {
    const datas: Data[] = [{icon: 'brush', text: 'Csúcs lefolyásának kirajzolása'}];

    const parentId = this.graph.find(x => x.id === id).parentId;
    if (parentId && parentId !== '') {
      datas.push({icon: 'search', text: 'Csúcshoz tartozó vezérlésfolyam gráf csúcs kijelölése'});
    }

    const bottomSheet = this.bottomSheet.open(BottomSheetTemplateComponent, {
      data: datas
    });

    bottomSheet.afterDismissed().subscribe(result => {
      if (result === 0) {
        this.drawPath(id);
      } else if (result === 1) {
        this.dataFlowGraphClick.emit(parentId);
      }
    });
  }

  // Szélességi bejárás szimulálása
  BFS() {
    if (this.BFSIsRun) {
      clearInterval(this.timer);
      this.BFSIsRun = false;
      return;
    }

    if (this.BFSQueue.length === 0) {
      this.colorNodes = this.graph;
      // tslint:disable-next-line: no-shadowed-variable
      this.colorNodes.forEach(node => {
        this.setNodeAttributes(node.id, Color.White, Color.Black);
        node.isColor = false;
        node.edges.forEach(edge => {
          this.setEdgeAttributes(edge, Color.Black);
        });
      });
      this.BFSQueue = new Queue<Node>();
      const node = this.colorNodes.find(x => x.number === 0);
      this.setNodeAttributes(node.id, Color.Gray);
      this.BFSQueue.enqueue(node);
    }

    this.BFSIsRun = true;
    this.timer = setInterval(() => {
      if (this.BFSQueue.length === 0) {
        this.stopBFS();
      } else {
        const node = this.BFSQueue.dequeue();

        node.edges.forEach(edge => {
          const childNode = this.colorNodes.find(x => x.id === edge.child);
          if (!childNode.isColor && !this.BFSQueue.toArray().find(x => x.id === childNode.id)) {
            this.setNodeAttributes(childNode.id, Color.Gray);
            childNode.isColor = true;
            this.BFSQueue.enqueue(childNode);
          }
        });

        this.setNodeAttributes(node.id, Color.Black, Color.White);
      }
    }, 1000);
  }

  stopBFS() {
    if (this.BFSIsRun) {
      clearInterval(this.timer);
      this.BFSIsRun = false;
      this.BFSEnd.emit();
    }
  }

  // Csúcshoz vezető út és onnan tovább menő csúcsok kiszínezése
  drawPath(id: string) {
    this.reset();
    const node = this.graph.find(x => x.id === id);
    this.colorNodes.push(node);
    let parentNodes = Copy<Node[]>(this.graph.filter(x => x.edges.filter(y => y.child === node.id).length > 0));
    parentNodes.forEach(x => {
      x.edges = new Array<Edge>();
      x.edges.push({child: id, parent: x.id, color: Color.Black, edgeArrowType: 0, edgeStyle: 0});
    });

    while (parentNodes.length > 0) {
      let newParentNodes = new Array<Node>();
      parentNodes.forEach(parentNode => {
        this.colorNodes.push(parentNode);
        // tslint:disable-next-line:max-line-length
        newParentNodes = newParentNodes.concat(Copy<Node[]>(this.graph.filter(x => x.edges.filter(y => y.child === parentNode.id).length > 0)));
      });

      for (let i = newParentNodes.length - 1; i >= 0; --i) {
        for (let j = newParentNodes[i].edges.length - 1; j >= 0; --j) {
          if (newParentNodes[i].edges[j] && parentNodes.filter(x => x.id === newParentNodes[i].edges[j].child).length === 0) {
            newParentNodes[i].edges.splice(j, 1);
          }
        }
      }
      parentNodes = newParentNodes;
    }

    let childNodes = new Array<Node>();
    node.edges.forEach(edge => {
      const childNode = this.graph.find(x => x.id === edge.child);
      this.colorNodes.push(childNode);
      childNodes.push(childNode);
    });
    while (childNodes.length !== 0) {
      const newchildNodes = new Array<Node>();
      childNodes.forEach(childNode => {
        childNode.edges.forEach(edge => {
          const newChildNode = this.graph.find(x => x.id === edge.child);
          this.colorNodes.push(newChildNode);
          newchildNodes.push(newChildNode);
        });
      });
      childNodes = newchildNodes;
    }

    this.colorNodes.forEach(colorNode => {
      this.setNodeAttributes(colorNode.id, Color.Blue, Color.Black);
      colorNode.edges.forEach(edge => {
        if (this.colorNodes.find(x => x.id === edge.child)) {
          this.setEdgeAttributes(edge, Color.Blue);
        }
      });
    });
  }

  // ControlFlowGraph 1 adott csúcsának kijelölése
  getNode = (id: string) => {
    return new Promise<Node>((resolve) => {
      const time = setInterval(() => {
          clearInterval(time);
          const node = this.graph.find(x => x.id === id);
          resolve(node);
      }, 1000);
    });
  }

  selectNode(id: string) {
    this.getNode(id).then(x => {
      this.reset();
      this.colorNodes.push(x);
      this.setNodeAttributes(id, Color.Blue);
    });
  }

  // Gráf visszaállítása az eredeti formára
  private reset() {
    if (this.BFSIsRun) {
      clearInterval(this.timer);
      this.BFSIsRun = false;
      this.BFSQueue = new Queue<Node>();
      this.BFSEnd.emit();
    }

    if (this.colorNodes && this.colorNodes.length !== 0) {
      this.colorNodes = new Array<Node>();
      this.BFSQueue = new Queue<Node>();
      this.graph.forEach(node => {
        node.isColor = false;
        this.setNodeAttributes(node.id, node.fillColor, node.fontColor);
        node.edges.forEach(edge => {
          this.setEdgeAttributes(edge, edge.color);
        });
      });
    }
  }

  // HTML attribútumok beállítása
  private setNodeAttributes(id: string, fillColor: string, textColor?: string) {
    const attributes = document.getElementById(id).children[1].children[0].children;
    attributes[0].setAttribute('fill', fillColor);
    if (textColor) {
      attributes[0].setAttribute('stroke', textColor);
      for (let i = 1; i < attributes.length; ++i) {
        attributes[i].setAttribute('fill', textColor);
      }
    }
  }

  private setEdgeAttributes(edge: Edge, color: string) {
    const attributes = document.getElementById(`${edge.parent} -> ${edge.child}`).children;
    attributes[1].setAttribute('stroke', color);
    if (edge.edgeArrowType === 0) {
      attributes[2].setAttribute('stroke', color);
      attributes[2].setAttribute('fill', color);
    }
  }
}
