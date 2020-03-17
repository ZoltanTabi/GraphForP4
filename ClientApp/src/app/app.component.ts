import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'd3-graphviz in Angular';
  list: Array<string> = ['start [fontcolor="#008000" shape="diamond"]' ,'node [style="filled"] start [fillcolor="#008000" shape="diamond"] start -> b',
     'node [style="filled"] start [fillcolor="#008000" shape="diamond"] start -> b; start -> c',
     'node [style="filled"] start [fillcolor="#008000" shape="diamond"] start -> b; start -> c; b -> d; c -> d'];
  numbers: Array<Number> = [1,2,3,4];

  ngOnInit() {
    
  }
}
