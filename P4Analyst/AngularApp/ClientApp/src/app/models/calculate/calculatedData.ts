import { Node } from '../graph/node';
import { BarChartData } from './barChartData';
import { PieChartData } from './pieChartData';

export interface CalculatedData {
  controlFlowGraphs: Array<Node[]>;
  dataFlowGraphs: Array<Node[]>;
  readAndWriteChartData: BarChartData;
  useVariable: PieChartData;
  useful: PieChartData;
  headers: BarChartData;
}
