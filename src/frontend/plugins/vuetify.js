import 'vuetify/styles'
import { createVuetify } from 'vuetify'

const vuetify = createVuetify({
  theme: {
    defaultTheme: 'light',
    themes: {
      light: {
        colors: {
          // Brand Colors
          primary: '#003E7A',    // WCTC Navy Blue
          secondary: '#0077C8',  // Bright Blue
          accent: '#FFD54F',     // Accent Yellow
          teal: '#00A6A6',       // Accent Teal
          purple: '#5F6DB3',     // Accent Purple

          // Backgrounds
          surface: '#F4F6F8',    // Light gray-blue surface
          background: '#FFFFFF', // Page background

          // Text
          'on-primary': '#FFFFFF',
          'on-secondary': '#FFFFFF',
          'on-surface': '#1A1A1A',
          'on-background': '#1A1A1A',

          // Feedback
          success: '#00A6A6',
          warning: '#FFD54F',
          error: '#D32F2F',
          info: '#0077C8',

          // Neutrals & borders
          border: '#DDE3EB',
          overlay: '#003E7A',
        },
      },
      dark: {
        colors: {
          primary: '#0077C8',
          secondary: '#5F6DB3',
          accent: '#FFD54F',
          teal: '#00A6A6',       // Accent Teal - same as light theme
          purple: '#5F6DB3',     // Accent Purple - same as secondary
          surface: '#1E1E1E',
          background: '#121212',
          'on-primary': '#FFFFFF',
          'on-secondary': '#FFFFFF',
          'on-surface': '#FFFFFF',
          'on-background': '#FFFFFF',
          success: '#00A6A6',
          warning: '#FFD54F',
          error: '#F44336',
          info: '#5F6DB3',
          border: '#404040',
          overlay: '#003E7A',
        },
      },
    },
  },
})

export default vuetify
