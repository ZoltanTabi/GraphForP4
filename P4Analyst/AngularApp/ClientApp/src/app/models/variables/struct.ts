import { Variable } from './variable';
import { Header } from './header';

export interface Struct {
  name: string;
  structs: Map<string, Struct>;
  headers: Map<string, Header>;
  variables: Map<string, Variable>;
}
