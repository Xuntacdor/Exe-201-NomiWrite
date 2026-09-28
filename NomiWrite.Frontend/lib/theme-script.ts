// Runs before paint. Only the appearance preference is stored, never user data.
export const themeScript = `(() => {
  let preference = null;
  try { preference = localStorage.getItem('nomiwrite_theme'); } catch {}
  const dark = preference === 'dark' || (preference !== 'light' && window.matchMedia('(prefers-color-scheme: dark)').matches);
  document.documentElement.dataset.theme = dark ? 'dark' : 'light';
})();`;
