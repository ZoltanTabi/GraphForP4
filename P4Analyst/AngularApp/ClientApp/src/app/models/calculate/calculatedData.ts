import { Node } from '../graph/node';
import { BarChartData } from './barChartData';
import { PieChartData } from './pieChartData';
import { FileData } from '../file';

export interface CalculatedData {
  controlFlowGraphs: Array<Node[]>;
  dataFlowGraphs: Array<Node[]>;
  readAndWriteChartData: BarChartData;
  useVariable: PieChartData;
  useful: PieChartData;
  headers: BarChartData;
  file: FileData;
}
