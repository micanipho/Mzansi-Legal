export default function OrganicBackground() {
  return (
    <div
      style={{
        position: 'fixed',
        inset: 0,
        overflow: 'hidden',
        pointerEvents: 'none',
        zIndex: -10,
      }}
      aria-hidden="true"
    >
      <div style={{
        position: 'absolute',
        top: '-10%',
        left: '-10%',
        width: '50vw',
        height: '50vw',
        background: '#5D7052',
        opacity: 0.05,
        filter: 'blur(100px)',
        borderRadius: '60% 40% 30% 70% / 60% 30% 70% 40%',
      }} />
      <div style={{
        position: 'absolute',
        top: '20%',
        right: '-5%',
        width: '40vw',
        height: '60vw',
        background: '#C18C5D',
        opacity: 0.05,
        filter: 'blur(120px)',
        borderRadius: '60% 40% 30% 70% / 60% 30% 70% 40%',
      }} />
      <div style={{
        position: 'absolute',
        bottom: '-20%',
        left: '10%',
        width: '60vw',
        height: '40vw',
        background: '#5D7052',
        opacity: 0.03,
        filter: 'blur(100px)',
        borderRadius: '60% 40% 30% 70% / 60% 30% 70% 40%',
      }} />
    </div>
  );
}
