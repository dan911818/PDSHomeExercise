import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PersonCardComponent } from './person-card.component';
import { PersonViewModel } from '../../models/person-view-model';

describe('PersonCardComponent', () => {
  let component: PersonCardComponent;
  let fixture: ComponentFixture<PersonCardComponent>;

  const mockPerson: PersonViewModel = {
    id: 1,
    firstName: 'John',
    lastName: 'Doe',
    dateOfBirth: '1990-01-01',
    department: 'IT'
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PersonCardComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(PersonCardComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Initialization', () => {
    it('should initialize without person input', () => {
      expect(component.person).toBeUndefined();
    });

    it('should accept person input', () => {
      component.person = mockPerson;
      expect(component.person).toEqual(mockPerson);
    });
  });

  describe('Event Emitters', () => {
    beforeEach(() => {
      component.person = mockPerson;
    });

    describe('onEdit', () => {
      it('should emit edit event', () => {
        spyOn(component.edit, 'emit');

        component.onEdit();

        expect(component.edit.emit).toHaveBeenCalled();
      });

      it('should emit edit event when person has no id', () => {
        const personWithoutId: PersonViewModel = {
          firstName: 'Test',
          lastName: 'Person',
          dateOfBirth: '1990-01-01',
          department: 'IT'
        };
        component.person = personWithoutId;
        spyOn(component.edit, 'emit');

        component.onEdit();

        expect(component.edit.emit).toHaveBeenCalled();
      });
    });

    describe('onDelete', () => {
      it('should emit delete event', () => {
        spyOn(component.delete, 'emit');

        component.onDelete();

        expect(component.delete.emit).toHaveBeenCalled();
      });

      it('should emit delete event for different people', () => {
        const differentPerson: PersonViewModel = {
          id: 2,
          firstName: 'Jane',
          lastName: 'Smith',
          dateOfBirth: '1985-06-15',
          department: 'HR'
        };
        component.person = differentPerson;
        spyOn(component.delete, 'emit');

        component.onDelete();

        expect(component.delete.emit).toHaveBeenCalled();
      });
    });

    describe('onSelect', () => {
      it('should emit select event', () => {
        spyOn(component.select, 'emit');

        component.onSelect();

        expect(component.select.emit).toHaveBeenCalled();
      });

      it('should emit select event when person has no id', () => {
        const personWithoutId: PersonViewModel = {
          firstName: 'Test',
          lastName: 'Person',
          dateOfBirth: '1990-01-01',
          department: 'IT'
        };
        component.person = personWithoutId;
        spyOn(component.select, 'emit');

        component.onSelect();

        expect(component.select.emit).toHaveBeenCalled();
      });
    });
  });

  describe('Edge Cases', () => {
    it('should handle component methods when person is undefined', () => {
      component.person = undefined as any;
      spyOn(component.edit, 'emit');
      spyOn(component.select, 'emit');
      spyOn(component.delete, 'emit');

      component.onEdit();
      component.onSelect();
      component.onDelete();

      expect(component.edit.emit).toHaveBeenCalled();
      expect(component.select.emit).toHaveBeenCalled();
      expect(component.delete.emit).toHaveBeenCalled();
    });
  });

  describe('Integration', () => {
    it('should work with complete person data flow', () => {
      const completePerson: PersonViewModel = {
        id: 999,
        firstName: 'Complete',
        lastName: 'Person',
        dateOfBirth: '1980-12-31',
        department: 'Finance'
      };

      component.person = completePerson;

      spyOn(component.edit, 'emit');
      spyOn(component.select, 'emit');
      spyOn(component.delete, 'emit');

      component.onEdit();
      component.onSelect();
      component.onDelete();

      expect(component.edit.emit).toHaveBeenCalled();
      expect(component.select.emit).toHaveBeenCalled();
      expect(component.delete.emit).toHaveBeenCalled();
    });
  });
});
