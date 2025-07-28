import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PersonListComponent } from './person-list.component';
import { PersonViewModel } from '../../models/person-view-model';

describe('PersonListComponent', () => {
  let component: PersonListComponent;
  let fixture: ComponentFixture<PersonListComponent>;

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
    await TestBed.configureTestingModule({
      declarations: [PersonListComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(PersonListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Initialization', () => {
    it('should initialize with empty people array', () => {
      expect(component.people).toEqual([]);
    });

    it('should accept people input', () => {
      component.people = mockPeople;
      expect(component.people).toEqual(mockPeople);
    });
  });

  describe('Event Emitters', () => {
    beforeEach(() => {
      component.people = mockPeople;
    });

    describe('onEdit', () => {
      it('should emit editPerson event with correct person', () => {
        spyOn(component.editPerson, 'emit');
        const personToEdit = mockPeople[0];

        component.onEdit(personToEdit);

        expect(component.editPerson.emit).toHaveBeenCalledWith(personToEdit);
      });
    });

    describe('onDelete', () => {
      it('should emit deletePerson event when user confirms deletion', () => {
        spyOn(window, 'confirm').and.returnValue(true);
        spyOn(component.deletePerson, 'emit');
        const personToDelete = mockPeople[0];

        component.onDelete(personToDelete);

        expect(window.confirm).toHaveBeenCalledWith('Are you sure you want to delete John Doe?');
        expect(component.deletePerson.emit).toHaveBeenCalledWith(personToDelete);
      });

      it('should not emit deletePerson event when user cancels deletion', () => {
        spyOn(window, 'confirm').and.returnValue(false);
        spyOn(component.deletePerson, 'emit');
        const personToDelete = mockPeople[0];

        component.onDelete(personToDelete);

        expect(window.confirm).toHaveBeenCalledWith('Are you sure you want to delete John Doe?');
        expect(component.deletePerson.emit).not.toHaveBeenCalled();
      });

      it('should show correct confirmation message for different people', () => {
        spyOn(window, 'confirm').and.returnValue(true);
        const personToDelete = mockPeople[1]; // Jane Smith

        component.onDelete(personToDelete);

        expect(window.confirm).toHaveBeenCalledWith('Are you sure you want to delete Jane Smith?');
      });
    });

    describe('onSelect', () => {
      it('should emit selectPerson event with correct person', () => {
        spyOn(component.selectPerson, 'emit');
        const personToSelect = mockPeople[1];

        component.onSelect(personToSelect);

        expect(component.selectPerson.emit).toHaveBeenCalledWith(personToSelect);
      });
    });
  });

  describe('Edge Cases', () => {
    it('should handle empty people array', () => {
      component.people = [];
      expect(component.people.length).toBe(0);
    });

    it('should handle person with missing optional id', () => {
      const personWithoutId: PersonViewModel = {
        firstName: 'Test',
        lastName: 'Person',
        dateOfBirth: '1990-01-01',
        department: 'IT'
      };

      spyOn(window, 'confirm').and.returnValue(true);
      spyOn(component.deletePerson, 'emit');

      component.onDelete(personWithoutId);

      expect(window.confirm).toHaveBeenCalledWith('Are you sure you want to delete Test Person?');
      expect(component.deletePerson.emit).toHaveBeenCalledWith(personWithoutId);
    });

    it('should handle person with empty names', () => {
      const personWithEmptyNames: PersonViewModel = {
        id: 3,
        firstName: '',
        lastName: '',
        dateOfBirth: '1990-01-01',
        department: 'IT'
      };

      spyOn(window, 'confirm').and.returnValue(true);

      component.onDelete(personWithEmptyNames);

      expect(window.confirm).toHaveBeenCalledWith('Are you sure you want to delete  ?');
    });
  });
});
