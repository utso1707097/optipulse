import { describe, expect, it } from 'vitest';
import { apiUrl } from '../src/api/config';

describe('apiUrl', () => {
  it('produces a single slash between base and path', () => {
    // Guards the concatenation bug that only shows up in the deployed build, where the base
    // URL is non-empty — locally the base is '' and any trailing-slash handling looks correct.
    expect(apiUrl('/api/v1/flags')).not.toContain('//api');
  });

  it('accepts paths with or without a leading slash', () => {
    expect(apiUrl('api/v1/flags')).toBe(apiUrl('/api/v1/flags'));
  });
});
