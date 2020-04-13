export interface Node {
  parentId?: string;
  node: [string, string];
  edges: Array<[[string, string], string]>;
  number: number;
}
