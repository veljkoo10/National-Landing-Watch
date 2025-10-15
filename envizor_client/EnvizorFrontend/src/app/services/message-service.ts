import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseService } from './base-service';
import { API_CONFIG } from './api-config';
import { MessageDto } from '../DTOs';

@Injectable({ providedIn: 'root' })
export class MessageService extends BaseService {
  private readonly messageUrl = `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.messages}`;

  constructor(http: HttpClient) {
    super(http);
  }

  sendMessage(message: MessageDto): Observable<MessageDto> {
    return this.post<MessageDto>(this.messageUrl, message);
  }

  getAllMessages(): Observable<MessageDto[]> {
    return this.get<MessageDto[]>(this.messageUrl);
  }

  getMessageById(id: number): Observable<MessageDto> {
    return this.get<MessageDto>(`${this.messageUrl}/${id}`);
  }

  deleteMessage(id: number): Observable<void> {
    return this.delete<void>(`${this.messageUrl}/${id}`);
  }
}