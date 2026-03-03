import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-info',
  imports: [],
  templateUrl: './info.html',
  styleUrl: './info.scss',
})
export class Info {
  @Output() closeClicked = new EventEmitter<void>();

  onClose() {
    this.closeClicked.emit();
  }
}
