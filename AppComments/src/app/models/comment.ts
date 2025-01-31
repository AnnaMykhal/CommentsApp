export interface Comment {
  id: number;
  userName: string;
  email: string;
  homePage: string | null;
  captcha: string;
  text: string;
  parentId: number | null;  // Для каскадних коментарів
  dateCreated: string;
  replies: Comment[];  // Відповіді на цей коментар
}
