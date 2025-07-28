import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { PersonViewModel } from '../models/person-view-model';

@Injectable({
  providedIn: 'root'
})
export class PersonService {
  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

  getAll(): Observable<PersonViewModel[]> {
    return this.http.get<any[]>(this.baseUrl + 'api/person/all').pipe(
      map(people => people.map(person => ({
        id: person.id,
        firstName: person.firstName,
        lastName: person.lastName,
        dateOfBirth: person.dateOfBirth,
        department: person.department?.name || 'No Department'
      })))
    );
  }

  getById(id: number): Observable<PersonViewModel> {
    return this.http.get<any>(this.baseUrl + `api/person/${id}`).pipe(
      map(person => ({
        id: person.id,
        firstName: person.firstName,
        lastName: person.lastName,
        dateOfBirth: person.dateOfBirth,
        department: person.department?.name || 'No Department'
      }))
    );
  }

  create(person: Omit<PersonViewModel, 'id'>): Observable<PersonViewModel> {
    // Transform PersonViewModel to match API expected format (PersonDTO)
    const personDto = {
      firstName: person.firstName,
      lastName: person.lastName,
      dateOfBirth: person.dateOfBirth,
      department: {
        name: person.department
      }
    };
    return this.http.post<any>(this.baseUrl + 'api/person', personDto).pipe(
      map(response => ({
        id: response.id,
        firstName: response.firstName,
        lastName: response.lastName,
        dateOfBirth: response.dateOfBirth,
        department: response.department?.name || 'No Department'
      }))
    );
  }

  update(id: number, person: PersonViewModel): Observable<PersonViewModel> {
    // Transform PersonViewModel to match API expected format (PersonDTO)
    const personDto = {
      id: person.id,
      firstName: person.firstName,
      lastName: person.lastName,
      dateOfBirth: person.dateOfBirth,
      department: {
        name: person.department
      }
    };
    return this.http.put<any>(this.baseUrl + `api/person/${id}`, personDto).pipe(
      map(response => ({
        id: response.id,
        firstName: response.firstName,
        lastName: response.lastName,
        dateOfBirth: response.dateOfBirth,
        department: response.department?.name || 'No Department'
      }))
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(this.baseUrl + `api/person/${id}`);
  }
}
