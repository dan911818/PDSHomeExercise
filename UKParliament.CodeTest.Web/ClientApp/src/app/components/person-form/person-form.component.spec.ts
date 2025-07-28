import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PersonFormComponent } from './person-form.component';
import { PersonViewModel } from '../../models/person-view-model';

describe('PersonFormComponent', () => {
  let component: PersonFormComponent;
  let fixture: ComponentFixture<PersonFormComponent>;

  const mockPerson: PersonViewModel = {
    id: 1,
    firstName: 'John',
    lastName: 'Doe',
    dateOfBirth: '1990-01-01',
    department: 'IT'
  };

  const mockDepartments = ['IT', 'HR', 'Finance', 'Marketing'];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PersonFormComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(PersonFormComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Initialization', () => {
    it('should initialize form with empty values when no person provided', () => {
      component.ngOnInit();

      expect(component.personForm.firstName).toBe('');
      expect(component.personForm.lastName).toBe('');
      expect(component.personForm.dateOfBirth).toBe('');
      expect(component.personForm.department).toBe('');
      expect(component.isEditing).toBeFalsy();
    });

    it('should initialize form with person data when person provided', () => {
      component.person = mockPerson;
      component.ngOnInit();

      expect(component.personForm.firstName).toBe('John');
      expect(component.personForm.lastName).toBe('Doe');
      expect(component.personForm.dateOfBirth).toBe('1990-01-01');
      expect(component.personForm.department).toBe('IT');
      expect(component.isEditing).toBeTruthy();
    });

    it('should accept departments input', () => {
      component.departments = mockDepartments;
      expect(component.departments).toEqual(mockDepartments);
    });

    it('should initialize with null person and empty departments', () => {
      expect(component.person).toBeNull();
      expect(component.departments).toEqual([]);
      expect(component.isVisible).toBeFalsy();
    });
  });

  describe('Form Validation', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should be invalid when all fields are empty', () => {
      expect(component.isFormValid()).toBeFalsy();
    });

    it('should be invalid when firstName is missing', () => {
      component.personForm = {
        firstName: '',
        lastName: 'Doe',
        dateOfBirth: '1990-01-01',
        department: 'IT'
      };

      expect(component.isFormValid()).toBeFalsy();
    });

    it('should be invalid when lastName is missing', () => {
      component.personForm = {
        firstName: 'John',
        lastName: '',
        dateOfBirth: '1990-01-01',
        department: 'IT'
      };

      expect(component.isFormValid()).toBeFalsy();
    });

    it('should be invalid when dateOfBirth is missing', () => {
      component.personForm = {
        firstName: 'John',
        lastName: 'Doe',
        dateOfBirth: '',
        department: 'IT'
      };

      expect(component.isFormValid()).toBeFalsy();
    });

    it('should be invalid when department is missing', () => {
      component.personForm = {
        firstName: 'John',
        lastName: 'Doe',
        dateOfBirth: '1990-01-01',
        department: ''
      };

      expect(component.isFormValid()).toBeFalsy();
    });

    it('should be valid when all required fields are provided', () => {
      component.personForm = {
        firstName: 'John',
        lastName: 'Doe',
        dateOfBirth: '1990-01-01',
        department: 'IT'
      };

      expect(component.isFormValid()).toBeTruthy();
    });
  });

  describe('Form Save', () => {
    beforeEach(() => {
      component.ngOnInit();
      spyOn(component.save, 'emit');
    });

    it('should emit save event with form data in add mode', () => {
      component.personForm = {
        firstName: 'John',
        lastName: 'Doe',
        dateOfBirth: '1990-01-01',
        department: 'IT'
      };

      component.onSave();

      expect(component.save.emit).toHaveBeenCalledWith({
        id: 0,
        firstName: 'John',
        lastName: 'Doe',
        dateOfBirth: '1990-01-01',
        department: 'IT'
      });
    });

    it('should emit save event with person id when editing existing person', () => {
      component.person = mockPerson;
      component.ngOnInit();

      component.personForm = {
        firstName: 'Jane',
        lastName: 'Smith',
        dateOfBirth: '1985-06-15',
        department: 'HR'
      };

      component.onSave();

      expect(component.save.emit).toHaveBeenCalledWith({
        id: 1,
        firstName: 'Jane',
        lastName: 'Smith',
        dateOfBirth: '1985-06-15',
        department: 'HR'
      });
    });

    it('should reset form after save', () => {
      component.personForm = {
        firstName: 'John',
        lastName: 'Doe',
        dateOfBirth: '1990-01-01',
        department: 'IT'
      };

      component.onSave();

      expect(component.personForm.firstName).toBe('');
      expect(component.personForm.lastName).toBe('');
      expect(component.personForm.dateOfBirth).toBe('');
      expect(component.personForm.department).toBe('');
    });
  });

  describe('Form Cancel', () => {
    it('should emit cancel event when onCancel is called', () => {
      spyOn(component.cancel, 'emit');

      component.onCancel();

      expect(component.cancel.emit).toHaveBeenCalled();
    });

    it('should reset form when onCancel is called', () => {
      component.personForm = {
        firstName: 'John',
        lastName: 'Doe',
        dateOfBirth: '1990-01-01',
        department: 'IT'
      };

      component.onCancel();

      expect(component.personForm.firstName).toBe('');
      expect(component.personForm.lastName).toBe('');
      expect(component.personForm.dateOfBirth).toBe('');
      expect(component.personForm.department).toBe('');
    });
  });

  describe('Date Handling', () => {
    it('should handle date with time component', () => {
      const personWithDateTime: PersonViewModel = {
        id: 1,
        firstName: 'John',
        lastName: 'Doe',
        dateOfBirth: '1990-01-01T00:00:00.000Z',
        department: 'IT'
      };

      component.person = personWithDateTime;
      component.ngOnInit();

      expect(component.personForm.dateOfBirth).toBe('1990-01-01');
    });

    it('should handle date without time component', () => {
      component.person = mockPerson;
      component.ngOnInit();

      expect(component.personForm.dateOfBirth).toBe('1990-01-01');
    });
  });

  describe('Edge Cases', () => {
    it('should handle person with missing optional id', () => {
      const personWithoutId: PersonViewModel = {
        firstName: 'Test',
        lastName: 'Person',
        dateOfBirth: '1990-01-01',
        department: 'IT'
      };

      component.person = personWithoutId;
      component.ngOnInit();
      spyOn(component.save, 'emit');

      component.onSave();

      expect(component.save.emit).toHaveBeenCalledWith({
        id: 0,
        firstName: 'Test',
        lastName: 'Person',
        dateOfBirth: '1990-01-01',
        department: 'IT'
      });
    });

    it('should handle empty departments array', () => {
      component.departments = [];
      expect(component.departments.length).toBe(0);
    });
  });

  describe('Integration', () => {
    it('should work through complete add flow', () => {
      component.departments = mockDepartments;
      component.ngOnInit();
      spyOn(component.save, 'emit');
      spyOn(component.cancel, 'emit');

      // Fill form
      component.personForm = {
        firstName: 'New',
        lastName: 'User',
        dateOfBirth: '1992-03-15',
        department: 'Marketing'
      };

      // Submit
      component.onSave();
      expect(component.save.emit).toHaveBeenCalledWith({
        id: 0,
        firstName: 'New',
        lastName: 'User',
        dateOfBirth: '1992-03-15',
        department: 'Marketing'
      });

      // Cancel
      component.onCancel();
      expect(component.cancel.emit).toHaveBeenCalled();
      expect(component.personForm.firstName).toBe('');
    });

    it('should work through complete edit flow', () => {
      component.person = mockPerson;
      component.departments = mockDepartments;
      component.ngOnInit();
      spyOn(component.save, 'emit');

      // Verify initial values
      expect(component.personForm.firstName).toBe('John');
      expect(component.isEditing).toBeTruthy();

      // Update form
      component.personForm = {
        firstName: 'Updated',
        lastName: 'Name',
        dateOfBirth: '1990-01-01',
        department: 'IT'
      };

      // Submit
      component.onSave();
      expect(component.save.emit).toHaveBeenCalledWith({
        id: 1,
        firstName: 'Updated',
        lastName: 'Name',
        dateOfBirth: '1990-01-01',
        department: 'IT'
      });
    });
  });
});
