import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';

interface CommunicationLogDto {
  id: string;
  senderId: string;
  recipientId?: string;
  channel: string;
  subject: string;
  content: string;
  status: string;
  errorMessage?: string;
  alumniName?: string;
  targetAddress?: string;
  creationTime: string;
}

interface Props {
  isOpen: boolean;
  onClose: () => void;
}

const PAGE_SIZE = 15;

const StatusBadge: React.FC<{ status: string }> = ({ status }) => {
  const config: Record<string, { color: string; bg: string; label: string }> = {
    Success: { color: '#10b981', bg: 'rgba(16,185,129,0.12)', label: '✓ Delivered' },
    Failed:  { color: '#ef4444', bg: 'rgba(239,68,68,0.12)',   label: '✕ Failed'    },
    Skipped: { color: '#f59e0b', bg: 'rgba(245,158,11,0.12)',  label: '⚠ Skipped'  },
  };
  const c = config[status] ?? { color: '#94a3b8', bg: 'rgba(148,163,184,0.12)', label: status };
  return (
    <span style={{
      display: 'inline-flex', alignItems: 'center', gap: 5,
      padding: '3px 10px', borderRadius: 20, fontSize: 12, fontWeight: 600,
      color: c.color, backgroundColor: c.bg, border: `1px solid ${c.color}33`
    }}>
      {c.label}
    </span>
  );
};

const CommunicationLogsModal: React.FC<Props> = ({ isOpen, onClose }) => {
  const [search, setSearch]       = useState('');
  const [channel, setChannel]     = useState('');
  const [status, setStatus]       = useState('');
  const [page, setPage]           = useState(0);
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const debounceRef = React.useRef<ReturnType<typeof setTimeout> | undefined>(undefined);

  const handleSearch = (v: string) => {
    setSearch(v);
    clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => { setDebouncedSearch(v); setPage(0); }, 400);
  };

  const { data, isLoading, isFetching } = useQuery({
    queryKey: ['communicationLogs', debouncedSearch, channel, status, page],
    queryFn: () => adminService.getCommunicationLogs({
      filterText: debouncedSearch || undefined,
      channel: channel || undefined,
      status: status || undefined,
      skipCount: page * PAGE_SIZE,
      maxResultCount: PAGE_SIZE,
    }),
    enabled: isOpen,
    placeholderData: (prev: any) => prev,
  });

  const typedData = data as { items: CommunicationLogDto[]; totalCount: number } | undefined;
  const items: CommunicationLogDto[] = typedData?.items ?? [];
  const total: number = typedData?.totalCount ?? 0;
  const totalPages = Math.ceil(total / PAGE_SIZE);

  if (!isOpen) return null;

  return (
    <div style={{
      position: 'fixed', inset: 0, zIndex: 9999,
      background: 'rgba(0,0,0,0.65)', backdropFilter: 'blur(6px)',
      display: 'flex', alignItems: 'center', justifyContent: 'center', padding: 20
    }} onClick={e => { if (e.target === e.currentTarget) onClose(); }}>
      <div style={{
        background: 'linear-gradient(145deg, #1a2035, #0f172a)',
        border: '1px solid rgba(255,255,255,0.1)',
        borderRadius: 16, width: '100%', maxWidth: 950,
        maxHeight: '90vh', display: 'flex', flexDirection: 'column',
        boxShadow: '0 25px 60px rgba(0,0,0,0.5)'
      }}>
        {/* Header */}
        <div style={{
          display: 'flex', alignItems: 'center', justifyContent: 'space-between',
          padding: '20px 28px', borderBottom: '1px solid rgba(255,255,255,0.08)'
        }}>
          <div>
            <h2 style={{ margin: 0, fontSize: 20, fontWeight: 700, color: '#f1f5f9' }}>
              📨 Delivery Logs
            </h2>
            <p style={{ margin: '4px 0 0', fontSize: 13, color: '#64748b' }}>
              {total} total entries
            </p>
          </div>
          <button onClick={onClose} style={{
            background: 'rgba(255,255,255,0.06)', border: '1px solid rgba(255,255,255,0.1)',
            borderRadius: 8, color: '#94a3b8', cursor: 'pointer', fontSize: 18,
            width: 36, height: 36, display: 'flex', alignItems: 'center', justifyContent: 'center'
          }}>✕</button>
        </div>

        {/* Filters */}
        <div style={{
          display: 'flex', gap: 12, padding: '16px 28px',
          borderBottom: '1px solid rgba(255,255,255,0.06)', flexWrap: 'wrap'
        }}>
          <input
            id="log-search"
            value={search}
            onChange={e => handleSearch(e.target.value)}
            placeholder="Search by name, email, subject..."
            style={{
              flex: 1, minWidth: 240, padding: '9px 14px',
              background: 'rgba(255,255,255,0.06)', border: '1px solid rgba(255,255,255,0.12)',
              borderRadius: 8, color: '#e2e8f0', fontSize: 13, outline: 'none'
            }}
          />
          <select
            id="log-channel-filter"
            value={channel}
            onChange={e => { setChannel(e.target.value); setPage(0); }}
            style={{
              padding: '9px 14px', background: 'rgba(255,255,255,0.06)',
              border: '1px solid rgba(255,255,255,0.12)', borderRadius: 8,
              color: '#e2e8f0', fontSize: 13, cursor: 'pointer', outline: 'none'
            }}
          >
            <option value="">All Channels</option>
            <option value="Email">Email</option>
            <option value="Sms">SMS</option>
          </select>
          <select
            id="log-status-filter"
            value={status}
            onChange={e => { setStatus(e.target.value); setPage(0); }}
            style={{
              padding: '9px 14px', background: 'rgba(255,255,255,0.06)',
              border: '1px solid rgba(255,255,255,0.12)', borderRadius: 8,
              color: '#e2e8f0', fontSize: 13, cursor: 'pointer', outline: 'none'
            }}
          >
            <option value="">All Statuses</option>
            <option value="Success">Delivered</option>
            <option value="Failed">Failed</option>
            <option value="Skipped">Skipped</option>
          </select>
        </div>

        {/* Table */}
        <div style={{ flex: 1, overflowY: 'auto', position: 'relative' }}>
          {(isLoading || isFetching) && (
            <div style={{
              position: 'absolute', inset: 0, display: 'flex', alignItems: 'center',
              justifyContent: 'center', background: 'rgba(0,0,0,0.4)', zIndex: 2
            }}>
              <div style={{ color: '#818cf8', fontSize: 14 }}>Loading…</div>
            </div>
          )}
          {!isLoading && items.length === 0 ? (
            <div style={{
              display: 'flex', flexDirection: 'column', alignItems: 'center',
              justifyContent: 'center', padding: 60, color: '#475569'
            }}>
              <div style={{ fontSize: 40, marginBottom: 12 }}>📭</div>
              <p style={{ margin: 0, fontSize: 14 }}>No delivery logs found</p>
              <p style={{ margin: '4px 0 0', fontSize: 12, color: '#334155' }}>Try sending a broadcast first</p>
            </div>
          ) : (
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
              <thead>
                <tr style={{ borderBottom: '1px solid rgba(255,255,255,0.06)' }}>
                  {['Recipient', 'Target Address', 'Channel', 'Subject', 'Status', 'Sent At'].map(h => (
                    <th key={h} style={{
                      padding: '10px 16px', textAlign: 'left', color: '#64748b',
                      fontWeight: 600, fontSize: 11, textTransform: 'uppercase',
                      letterSpacing: '0.05em', whiteSpace: 'nowrap'
                    }}>{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {items.map((log, i) => (
                  <tr key={log.id} style={{
                    borderBottom: '1px solid rgba(255,255,255,0.04)',
                    background: i % 2 === 0 ? 'transparent' : 'rgba(255,255,255,0.02)',
                    transition: 'background 0.15s'
                  }}
                    onMouseEnter={e => (e.currentTarget.style.background = 'rgba(99,102,241,0.08)')}
                    onMouseLeave={e => (e.currentTarget.style.background = i % 2 === 0 ? 'transparent' : 'rgba(255,255,255,0.02)')}
                  >
                    <td style={{ padding: '12px 16px', color: '#e2e8f0', fontWeight: 500 }}>
                      {log.alumniName ?? '—'}
                    </td>
                    <td style={{ padding: '12px 16px', color: '#94a3b8', fontFamily: 'monospace', fontSize: 12 }}>
                      {log.targetAddress ?? '—'}
                    </td>
                    <td style={{ padding: '12px 16px' }}>
                      <span style={{
                        padding: '2px 8px', borderRadius: 4, fontSize: 11, fontWeight: 600,
                        color: log.channel === 'Email' ? '#818cf8' : '#34d399',
                        background: log.channel === 'Email' ? 'rgba(129,140,248,0.1)' : 'rgba(52,211,153,0.1)'
                      }}>
                        {log.channel === 'Email' ? '✉ Email' : '📱 SMS'}
                      </span>
                    </td>
                    <td style={{ padding: '12px 16px', color: '#cbd5e1', maxWidth: 180 }}>
                      <div style={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                        {log.subject || '—'}
                      </div>
                    </td>
                    <td style={{ padding: '12px 16px' }}>
                      <StatusBadge status={log.status} />
                      {log.errorMessage && (
                        <div style={{ fontSize: 11, color: '#ef4444', marginTop: 3 }} title={log.errorMessage}>
                          {log.errorMessage.length > 40 ? log.errorMessage.slice(0, 40) + '…' : log.errorMessage}
                        </div>
                      )}
                    </td>
                    <td style={{ padding: '12px 16px', color: '#64748b', whiteSpace: 'nowrap', fontSize: 12 }}>
                      {new Date(log.creationTime).toLocaleString()}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <div style={{
            display: 'flex', alignItems: 'center', justifyContent: 'space-between',
            padding: '12px 28px', borderTop: '1px solid rgba(255,255,255,0.06)'
          }}>
            <span style={{ fontSize: 12, color: '#64748b' }}>
              Page {page + 1} of {totalPages} — {total} records
            </span>
            <div style={{ display: 'flex', gap: 6 }}>
              <button
                onClick={() => setPage(p => Math.max(0, p - 1))}
                disabled={page === 0}
                style={{
                  padding: '6px 14px', borderRadius: 6, fontSize: 12, cursor: page === 0 ? 'not-allowed' : 'pointer',
                  background: 'rgba(255,255,255,0.06)', border: '1px solid rgba(255,255,255,0.1)',
                  color: page === 0 ? '#334155' : '#e2e8f0', opacity: page === 0 ? 0.5 : 1
                }}
              >← Prev</button>
              <button
                onClick={() => setPage(p => Math.min(totalPages - 1, p + 1))}
                disabled={page >= totalPages - 1}
                style={{
                  padding: '6px 14px', borderRadius: 6, fontSize: 12,
                  cursor: page >= totalPages - 1 ? 'not-allowed' : 'pointer',
                  background: 'rgba(99,102,241,0.2)', border: '1px solid rgba(99,102,241,0.3)',
                  color: page >= totalPages - 1 ? '#334155' : '#818cf8',
                  opacity: page >= totalPages - 1 ? 0.5 : 1
                }}
              >Next →</button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default CommunicationLogsModal;
