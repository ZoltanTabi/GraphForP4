import { Variable } from './variable';

export interface Header {
  name: string;
  variables: Array<Variable>;
  valid: boolean;
}
