import { Node } from './graph/node';

export interface TabContent {
  name: string;
  id: string;
  graph: Array<Node>;
}
