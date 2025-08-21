import { render } from '@testing-library/react';
import React from 'react';

describe('Smoke', () => {
  it('runs a basic test', () => {
    const { container } = render(<div>ok</div>);
    expect(container).toHaveTextContent('ok');
  });
});


