import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  // list: Array<string> = ['start [fontcolor="#008000" shape="diamond"]' ,
  //  'node [style="filled"] start [fillcolor="#008000" shape="diamond"] start -> b',
  //  'node [style="filled"] start [fillcolor="#008000" shape="diamond"] start -> b; start -> c',
  //  'node [style="filled"] start [fillcolor="#008000" shape="diamond"] start -> b; start -> c; b -> d; c -> d'];

  // tslint:disable-next-line: use-lifecycle-interface
  ngOnInit() { }
}