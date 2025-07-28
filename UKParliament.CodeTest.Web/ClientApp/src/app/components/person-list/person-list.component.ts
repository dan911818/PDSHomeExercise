import { Component, Input, Output, EventEmitter } from '@angular/core';
import { PersonViewModel } from 'src/app/models/person-view-model';

@Component({
  selector: 'app-person-list',
  templateUrl: './person-list.component.html',
  styleUrls: ['./person-list.component.scss']
})
export class PersonListComponent {
  @Input() people: PersonViewModel[] = [];
  @Output() editPerson = new EventEmitter<PersonViewModel>();
  @Output() deletePerson = new EventEmitter<PersonViewModel>();
  @Output() selectPerson = new EventEmitter<PersonViewModel>();

  onEdit(person: PersonViewModel): void {
    this.editPerson.emit(person);
  }

  onDelete(person: PersonViewModel): void {
    if (confirm(`Are you sure you want to delete ${person.firstName} ${person.lastName}?`)) {
      this.deletePerson.emit(person);
    }
  }

  onSelect(person: PersonViewModel): void {
    this.selectPerson.emit(person);
  }
}
