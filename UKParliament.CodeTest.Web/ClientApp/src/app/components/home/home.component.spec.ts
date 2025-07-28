import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { HomeComponent } from './home.component';
import { PersonService } from '../../services/person.service';
import { PersonViewModel } from '../../models/person-view-model';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;
  let mockPersonService: jasmine.SpyObj<PersonService>;

  const mockPeople: PersonViewModel[] = [
    {
      id: 1,
      firstName: 'John',
      lastName: 'Doe',
      dateOfBirth: '1990-01-01',
      department: 'IT'
    },
    {
      id: 2,
      firstName: 'Jane',
      lastName: 'Smith',
      dateOfBirth: '1985-06-15',
      department: 'HR'
    }
  ];

  beforeEach(async () => {
    const personServiceSpy = jasmine.createSpyObj('PersonService', [
      'getAll', 'getById', 'create', 'update', 'delete'
    ]);

    await TestBed.configureTestingModule({
      declarations: [HomeComponent],
      providers: [
        { provide: PersonService, useValue: personServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
    mockPersonService = TestBed.inject(PersonService) as jasmine.SpyObj<PersonService>;

    // Default setup for getAll
    mockPersonService.getAll.and.returnValue(of(mockPeople));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Initialization', () => {
    it('should initialize with correct default values', () => {
      expect(component.people).toEqual([]);
      expect(component.departments).toEqual(['IT', 'HR', 'Finance', 'Operations', 'Legal']);
      expect(component.showForm).toBeFalse();
      expect(component.editingPerson).toBeNull();
    });

    it('should load people on init', () => {
      component.ngOnInit();

      expect(mockPersonService.getAll).toHaveBeenCalled();
      expect(component.people).toEqual(mockPeople);
    });

    it('should handle error when loading people fails', () => {
      const consoleSpy = spyOn(console, 'error');
      mockPersonService.getAll.and.returnValue(throwError('Network error'));

      component.ngOnInit();

      expect(consoleSpy).toHaveBeenCalledWith('Error loading people: Network error');
    });
  });

  describe('loadPeople', () => {
    it('should call PersonService.getAll and update people array', () => {
      component.loadPeople();

      expect(mockPersonService.getAll).toHaveBeenCalled();
      expect(component.people).toEqual(mockPeople);
    });

    it('should log successful load', () => {
      const consoleSpy = spyOn(console, 'info');
      
      component.loadPeople();

      expect(consoleSpy).toHaveBeenCalledWith(`Users loaded: ${JSON.stringify(mockPeople)}`);
    });
  });

  describe('Form Management', () => {
    it('should show form for adding new person', () => {
      component.onAddPerson();

      expect(component.editingPerson).toBeNull();
      expect(component.showForm).toBeTrue();
    });

    it('should show form for editing existing person', () => {
      const personToEdit = mockPeople[0];

      component.onEditPerson(personToEdit);

      expect(component.editingPerson).toEqual(personToEdit);
      expect(component.showForm).toBeTrue();
    });

    it('should hide form and clear editing person on cancel', () => {
      component.showForm = true;
      component.editingPerson = mockPeople[0];

      component.onCancelForm();

      expect(component.showForm).toBeFalse();
      expect(component.editingPerson).toBeNull();
    });
  });

  describe('Person Management', () => {
    beforeEach(() => {
      mockPersonService.create.and.returnValue(of(mockPeople[0]));
      mockPersonService.update.and.returnValue(of(mockPeople[0]));
      mockPersonService.delete.and.returnValue(of(undefined));
    });

    describe('onSavePerson', () => {
      it('should create new person when not editing', () => {
        const newPerson: PersonViewModel = {
          id: 0,
          firstName: 'New',
          lastName: 'Person',
          dateOfBirth: '1995-01-01',
          department: 'Finance'
        };
        component.editingPerson = null;

        component.onSavePerson(newPerson);

        expect(mockPersonService.create).toHaveBeenCalledWith({
          firstName: 'New',
          lastName: 'Person',
          dateOfBirth: '1995-01-01',
          department: 'Finance'
        });
        expect(mockPersonService.getAll).toHaveBeenCalled();
        expect(component.showForm).toBeFalse();
        expect(component.editingPerson).toBeNull();
      });

      it('should update existing person when editing', () => {
        const updatedPerson: PersonViewModel = {
          id: 1,
          firstName: 'Updated',
          lastName: 'Person',
          dateOfBirth: '1990-01-01',
          department: 'IT'
        };
        component.editingPerson = mockPeople[0];

        component.onSavePerson(updatedPerson);

        expect(mockPersonService.update).toHaveBeenCalledWith(1, updatedPerson);
        expect(mockPersonService.getAll).toHaveBeenCalled();
        expect(component.showForm).toBeFalse();
        expect(component.editingPerson).toBeNull();
      });

      it('should handle create error with alert', () => {
        const alertSpy = spyOn(window, 'alert');
        const consoleSpy = spyOn(console, 'error');
        const errorResponse = { error: { message: 'Validation failed' } };
        mockPersonService.create.and.returnValue(throwError(errorResponse));

        const newPerson: PersonViewModel = {
          id: 0,
          firstName: 'Invalid',
          lastName: 'Person',
          dateOfBirth: '2010-01-01', // Too young
          department: 'IT'
        };

        component.onSavePerson(newPerson);

        expect(consoleSpy).toHaveBeenCalled();
        expect(alertSpy).toHaveBeenCalledWith('Failed to create person: Validation failed');
      });

      it('should handle update error with alert', () => {
        const alertSpy = spyOn(window, 'alert');
        const consoleSpy = spyOn(console, 'error');
        const errorResponse = { error: { message: 'Update failed' } };
        mockPersonService.update.and.returnValue(throwError(errorResponse));

        component.editingPerson = mockPeople[0];
        const updatedPerson = { ...mockPeople[0], firstName: 'Updated' };

        component.onSavePerson(updatedPerson);

        expect(consoleSpy).toHaveBeenCalled();
        expect(alertSpy).toHaveBeenCalledWith('Failed to update person: Update failed');
      });
    });

    describe('onDeletePerson', () => {
      it('should delete person successfully', () => {
        const personToDelete = mockPeople[0];

        component.onDeletePerson(personToDelete);

        expect(mockPersonService.delete).toHaveBeenCalledWith(1);
        expect(mockPersonService.getAll).toHaveBeenCalled();
      });

      it('should handle delete error', () => {
        const consoleSpy = spyOn(console, 'error');
        mockPersonService.delete.and.returnValue(throwError('Delete failed'));
        const personToDelete = mockPeople[0];

        component.onDeletePerson(personToDelete);

        expect(consoleSpy).toHaveBeenCalledWith('Error deleting person: Delete failed');
      });

      it('should not delete person without id', () => {
        const personWithoutId: PersonViewModel = {
          firstName: 'Test',
          lastName: 'Person',
          dateOfBirth: '1990-01-01',
          department: 'IT'
        };

        component.onDeletePerson(personWithoutId);

        expect(mockPersonService.delete).not.toHaveBeenCalled();
      });
    });

    describe('onSelectPerson', () => {
      it('should log selected person details', () => {
        const consoleSpy = spyOn(console, 'log');
        const personToSelect = mockPeople[0];

        component.onSelectPerson(personToSelect);

        expect(consoleSpy).toHaveBeenCalledWith('Person selected:', 'John', 'Doe');
      });
    });
  });

  describe('Exercise Methods', () => {
    describe('onStartExercise', () => {
      it('should call getPersonById with id 1', () => {
        spyOn(component, 'getPersonById');

        component.onStartExercise();

        expect(component.getPersonById).toHaveBeenCalledWith(1);
      });
    });

    describe('getPersonById', () => {
      it('should fetch person by id and log result', () => {
        const consoleSpy = spyOn(console, 'info');
        mockPersonService.getById.and.returnValue(of(mockPeople[0]));

        component.getPersonById(1);

        expect(mockPersonService.getById).toHaveBeenCalledWith(1);
        expect(consoleSpy).toHaveBeenCalledWith(`User returned: ${JSON.stringify(mockPeople[0])}`);
      });

      it('should handle getById error', () => {
        const consoleSpy = spyOn(console, 'error');
        mockPersonService.getById.and.returnValue(throwError('Person not found'));

        component.getPersonById(999);

        expect(consoleSpy).toHaveBeenCalledWith('Error: Person not found');
      });
    });
  });
});
