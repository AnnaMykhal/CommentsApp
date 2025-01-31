import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CommentPaginationComponent } from './comment-pagination.component';

describe('CommentPaginationComponent', () => {
  let component: CommentPaginationComponent;
  let fixture: ComponentFixture<CommentPaginationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CommentPaginationComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CommentPaginationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
