import { Struct } from '../variables/struct';
import { FormControl } from '@angular/forms';
import { StructGroup } from './structGroup';

export interface Selector {
  startStruct: Array<Struct>;
  endStruct: Array<Struct>;
  startControl: FormControl;
  endControl: FormControl;
  id: number;
}
