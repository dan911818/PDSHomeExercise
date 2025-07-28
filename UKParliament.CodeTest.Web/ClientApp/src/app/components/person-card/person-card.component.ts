import { Component, Input, Output, EventEmitter } from '@angular/core';
import { PersonViewModel } from 'src/app/models/person-view-model';

@Component({
  selector: 'app-person-card',
  templateUrl: './person-card.component.html',
  styleUrls: ['./person-card.component.scss']
})
export class PersonCardComponent {
  @Input() person!: PersonViewModel;
  @Output() edit = new EventEmitter<void>();
  @Output() delete = new EventEmitter<void>();
  @Output() select = new EventEmitter<void>();

  onEdit(): void {
    this.edit.emit();
  }

  onDelete(): void {
    this.delete.emit();
  }

  onSelect(): void {
    this.select.emit();
  }
}
