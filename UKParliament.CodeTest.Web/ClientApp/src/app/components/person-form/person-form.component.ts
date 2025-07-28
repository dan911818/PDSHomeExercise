import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { PersonViewModel } from 'src/app/models/person-view-model';

@Component({
  selector: 'app-person-form',
  templateUrl: './person-form.component.html',
  styleUrls: ['./person-form.component.scss']
})
export class PersonFormComponent implements OnInit {
  @Input() person: PersonViewModel | null = null;
  @Input() departments: string[] = [];
  @Input() isVisible: boolean = false;
  @Output() save = new EventEmitter<PersonViewModel>();
  @Output() cancel = new EventEmitter<void>();

  isEditing: boolean = false;
  
  // Form model
  personForm = {
    firstName: '',
    lastName: '',
    dateOfBirth: '',
    department: ''
  };

  ngOnInit(): void {
    this.updateForm();
  }

  ngOnChanges(): void {
    this.updateForm();
  }

  private updateForm(): void {
    if (this.person) {
      this.isEditing = true;
      this.personForm = {
        firstName: this.person.firstName,
        lastName: this.person.lastName,
        dateOfBirth: this.person.dateOfBirth.includes('T') 
          ? this.person.dateOfBirth.split('T')[0] 
          : this.person.dateOfBirth,
        department: this.person.department
      };
    } else {
      this.isEditing = false;
      this.resetForm();
    }
  }

  onSave(): void {
    const personData: PersonViewModel = {
      id: this.person?.id || 0,
      firstName: this.personForm.firstName,
      lastName: this.personForm.lastName,
      dateOfBirth: this.personForm.dateOfBirth,
      department: this.personForm.department
    };
    
    this.save.emit(personData);
    this.resetForm();
  }

  onCancel(): void {
    this.cancel.emit();
    this.resetForm();
  }

  private resetForm(): void {
    this.personForm = {
      firstName: '',
      lastName: '',
      dateOfBirth: '',
      department: ''
    };
  }

  isFormValid(): boolean {
    return this.personForm.firstName.trim() !== '' &&
           this.personForm.lastName.trim() !== '' &&
           this.personForm.dateOfBirth !== '' &&
           this.personForm.department !== '';
  }
}
