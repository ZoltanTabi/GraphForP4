import { Component, OnInit, Input } from '@angular/core';
import { graphviz } from 'd3-graphviz';
import * as d3 from "d3";

@Component({
  selector: 'graph',
  templateUrl: './graph.component.html',
  styleUrls: ['./graph.component.scss']
})
export class GraphComponent implements OnInit {

  @Input() number: number;
  @Input() list: Array<string>;

  constructor() { }

  ngOnInit(): void {
    var _this = this;
    graphviz('#graph').transition(() => d3.transition().duration(500))
      .renderDot('digraph { ' + this.list[this.number] + ' } ', function() {
        _this.graphDrawing(++_this.number, this);
      });
  }

  public graphDrawing(i :number, graph: any) {
    if(i != this.list.length){
      var _this = this;
      graph.renderDot('digraph { ' + _this.list[i] + ' } ', function () {
        _this.graphDrawing(++i, this)
      });
    }
    else{
      console.log(graph);

      var nodes = d3.selectAll(".node");
      nodes.on("click", this.fieldClickHandler);
    }
  };

  private fieldClickHandler() {
    console.log(d3.event.toElement.__data__.parent.key);
  }

}
