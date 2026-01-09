import { ReactNode } from 'react';
import styles from '../CardLedger.module.scss';

type SectionCardProps = {
  title: string;
  subtitle?: string;
  children: ReactNode;
};

const SectionCard = ({ title, subtitle, children }: SectionCardProps) => {
  return (
    <section className={styles.card}>
      <header className={styles.cardHeader}>
        <h3 className={styles.cardTitle}>{title}</h3>
        {subtitle ? <p className={styles.cardSubtitle}>{subtitle}</p> : null}
      </header>
      <div className={styles.cardBody}>{children}</div>
    </section>
  );
};

export default SectionCard;
