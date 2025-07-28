import { Component, OnInit } from '@angular/core';
import { PersonService } from '../../services/person.service';
import { PersonViewModel } from 'src/app/models/person-view-model';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  people: PersonViewModel[] = [];
  departments: string[] = ['IT', 'HR', 'Finance', 'Operations', 'Legal'];
  
  // Form state
  showForm: boolean = false;
  editingPerson: PersonViewModel | null = null;

  constructor(private personService: PersonService) {}

  ngOnInit(): void {
    this.loadPeople();
  }

  loadPeople(): void {
    this.personService.getAll().subscribe({
      next: (result: PersonViewModel[]) => {
        this.people = result;
        console.info(`Users loaded: ${JSON.stringify(result)}`);
      },
      error: (e) => console.error(`Error loading people: ${e}`)
    });
  }

  onAddPerson(): void {
    this.editingPerson = null;
    this.showForm = true;
  }

  onEditPerson(person: PersonViewModel): void {
    this.editingPerson = person;
    this.showForm = true;
  }

  onDeletePerson(person: PersonViewModel): void {
    if (person.id) {
      this.personService.delete(person.id).subscribe({
        next: () => {
          this.loadPeople();
        },
        error: (e) => console.error(`Error deleting person: ${e}`)
      });
    }
  }

  onSelectPerson(person: PersonViewModel): void {
    console.log('Person selected:', person.firstName, person.lastName);
  }

  onSavePerson(personData: PersonViewModel): void {
    if (this.editingPerson && personData.id) {
      // Update existing person
      this.personService.update(personData.id, personData).subscribe({
        next: () => {
          this.loadPeople();
          this.onCancelForm();
        },
        error: (error) => {
          console.error(`Error updating person:`, error);
          // Extract error message from the response
          const errorMessage = error.error?.message || error.error || error.message || 'Unknown error occurred';
          alert(`Failed to update person: ${errorMessage}`);
        }
      });
    } else {
      // Create new person
      const newPerson = { ...personData };
      delete newPerson.id; // Remove id for creation
      
      this.personService.create(newPerson).subscribe({
        next: () => {
          this.loadPeople();
          this.onCancelForm();
        },
        error: (error) => {
          console.error(`Error creating person:`, error);
          // Extract error message from the response
          const errorMessage = error.error?.message || error.error || error.message || 'Unknown error occurred';
          alert(`Failed to create person: ${errorMessage}`);
        }
      });
    }
  }

  onCancelForm(): void {
    this.showForm = false;
    this.editingPerson = null;
  }

  onStartExercise(): void {
    this.getPersonById(1);
  }

  getPersonById(id: number): void {
    console.log(`Fetching person with ID: ${id}`);
    this.personService.getById(id).subscribe({
      next: (result: PersonViewModel) => console.info(`User returned: ${JSON.stringify(result)}`),
      error: (e) => console.error(`Error: ${e}`)
    });
  }
}
