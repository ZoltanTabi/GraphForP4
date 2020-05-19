import { Node } from '../graph/node';
import { BarChartData } from './barChartData';

export interface CalculatedData {
  controlFlowGraphs: Array<Node[]>;
  dataFlowGraphs: Array<Node[]>;
  barChartData: BarChartData;
}
