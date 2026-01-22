import { Typography, Container, Box } from '@mui/material';

function App() {
  return (
    <Container maxWidth="lg">
      <Box sx={{ my: 4, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
        <Typography variant="h3" component="h1" gutterBottom>
          Informatique Alumni
        </Typography>
        <Typography variant="subtitle1" color="text.secondary">
          Frontend Initialized successfully.
        </Typography>
      </Box>
    </Container>
  );
}

export default App;
