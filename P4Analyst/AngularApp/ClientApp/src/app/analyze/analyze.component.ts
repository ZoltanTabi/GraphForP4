import { Component, OnInit } from '@angular/core';
import { AnalyzeService } from './analyze.service';
import { Struct } from '../models/variables/struct';

@Component({
  selector: 'app-analyze',
  templateUrl: './analyze.component.html',
  providers: [AnalyzeService],
  styleUrls: ['./analyze.component.scss']
})
export class AnalyzeComponent implements OnInit {

  public structs: Array<Struct>;

  constructor(private analyzeService: AnalyzeService) { }

  ngOnInit() {
    this.analyzeService.getVariables().subscribe(result => {
      console.log(result);
      this.structs = result;
    });
  }

  onPut() {
    this.structs.forEach(struct => {
      struct.variables.forEach(variable => {
        variable.isInitialize = true;
      });
    });

    this.analyzeService.putStructs(this.structs).subscribe(result => {
      console.log(result);
    });
  }
}
