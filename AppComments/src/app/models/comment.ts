export interface Comment {
  commentId: string;
  username: string;
  email: string;
  homePage: string | null;
  captcha: string;
  content: string;
  parentCommentId: string | null;  // Для каскадних коментарів
  createdAtFormatted: string;
  replies: Comment[];  // Відповіді на цей коментар
  depth?: number;
  expanded?: boolean;
  replyCount: number;
}
