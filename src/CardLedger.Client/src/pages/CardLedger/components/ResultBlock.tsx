import styles from '../CardLedger.module.scss';

type ResultBlockProps = {
  title: string;
  status: string;
  data?: Record<string, unknown>;
  error?: string;
};

const ResultBlock = ({ title, status, data, error }: ResultBlockProps) => {
  const body = error
    ? error
    : data
      ? JSON.stringify(data, null, 2)
      : status === 'loading'
        ? 'Loading...'
        : 'No response yet.';

  return (
    <div className={styles.resultBlock}>
      <div className={styles.resultHeader}>
        <span>{title}</span>
        <span className={styles.resultStatus}>{status}</span>
      </div>
      <pre className={styles.resultBody}>{body}</pre>
    </div>
  );
};

export default ResultBlock;
