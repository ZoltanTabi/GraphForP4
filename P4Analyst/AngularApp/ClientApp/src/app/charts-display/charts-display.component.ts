import { Component, OnInit, Input } from '@angular/core';
import { ChartDataSets, ChartOptions } from 'chart.js';
import { CalculatedData } from '../models/calculate/calculatedData';

@Component({
  selector: 'app-charts-display',
  templateUrl: './charts-display.component.html',
  styleUrls: ['./charts-display.component.scss']
})
export class ChartsDisplayComponent implements OnInit {

  @Input() public set calculatedDataSetter(calculatedData: CalculatedData) {
    if (calculatedData && this.calculatedData !== calculatedData) {
      this.calculatedData = calculatedData;
      this.readAndWriteChartData = [];
      // tslint:disable-next-line:forin
      for (const label in this.calculatedData.readAndWriteChartData.doubleDatas) {
        this.readAndWriteChartData.push( { data: this.calculatedData.readAndWriteChartData.doubleDatas[label], label: label } );
      }

      this.headersChartData = [];
      // tslint:disable-next-line:forin
      for (const label in this.calculatedData.headers.doubleDatas) {
        this.headersChartData.push( { data: this.calculatedData.headers.doubleDatas[label], label: label } );
      }
    }
  }

  public calculatedData: CalculatedData;
  public readAndWriteChartData: ChartDataSets[];
  public headersChartData: ChartDataSets[];
  public cols: number;

  public barChartOptions: ChartOptions = {
    responsive: true,
    scales: { xAxes: [{}], yAxes: [{}] },
    plugins: {
      datalabels: {
        anchor: 'end',
        align: 'end',
      }
    }
  };

  public pieChartOptions: ChartOptions = {
    responsive: true,
    legend: {
      position: 'top',
    },
    plugins: {
      datalabels: {
        formatter: (_value: any, ctx: { chart: { data: { labels: { [x: string]: any; }; }; }; dataIndex: string | number; }) => {
          const label = ctx.chart.data.labels[ctx.dataIndex];
          return label;
        },
      },
    }
  };

  constructor() { }

  ngOnInit() {
    this.onResize();
  }

  onResize() {
    this.cols = (window.innerWidth <= 700) ? 1 : 2;
  }

  getControlFlowGraphs() {
    return this.calculatedData && this.calculatedData.controlFlowGraphs ? this.calculatedData.controlFlowGraphs : new Array<Node[]>();
  }

  getDataFlowGraphs() {
    return this.calculatedData && this.calculatedData.dataFlowGraphs ? this.calculatedData.dataFlowGraphs : new Array<Node[]>();
  }

}
