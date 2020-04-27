import { Edge } from './edge';

export interface Node {
  id: string;
  number: number;
  text: string;
  parentId: string;
  subGraph: string;
  tooltip: string;
  edges: Edge[];
  fillColor: string;
  fontColor: string;
  shape: number;
  isColor: boolean;
}
