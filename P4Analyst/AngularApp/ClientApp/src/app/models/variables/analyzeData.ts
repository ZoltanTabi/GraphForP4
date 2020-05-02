import { Struct } from './struct';

export interface AnalyzeData {
  id: number;
  startState: Array<Struct>;
  endState: Array<Struct>;
}
