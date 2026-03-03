import { Component, EventEmitter, Output, Signal, signal, WritableSignal } from '@angular/core';

@Component({
  selector: 'app-header',
  imports: [],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header {
  @Output() infoClicked = new EventEmitter<void>();

  onClick() {
    this.infoClicked.emit();
  }
}
