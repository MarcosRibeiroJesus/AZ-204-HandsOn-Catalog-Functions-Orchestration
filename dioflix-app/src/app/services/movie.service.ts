import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Movie {
  id: string;
  title: string;
  year: string;
  thumb: string;
  video: string;
}

@Injectable({
  providedIn: 'root'
})
export class MovieService {
  // private apiUrl = 'https://your-api-url.azurewebsites.net/api/'; // Replace with your actual API URL

  constructor(private http: HttpClient) {}

  getMovies(): Observable<Movie[]> {
    return this.http.get<Movie[]>(`http://localhost:7222/api/movies`);
  }

  getMovieById(id: string): Observable<Movie> {
    console.log("url", `http://localhost:7291/api/movieDetail?id=${id}`);
    return this.http.get<Movie>(`http://localhost:7291/api/movieDetail?id=${id}`);
  }
}
