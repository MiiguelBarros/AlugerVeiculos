import { definePreset } from '@primeuix/themes'
import Aura from '@primeuix/themes/aura'

const AppPreset = definePreset(Aura, {
  primitive: {
    borderRadius: {
      none: '0',
      xs: '4px',
      sm: '8px',
      md: '12px',
      lg: '16px',
      xl: '16px',
    },
    brand: {
      50: '#fff3ec',
      100: '#ffe2d1',
      200: '#ffc3a1',
      300: '#ff9e66',
      400: '#ff7633',
      500: '#ff5000',
      600: '#e64800',
      700: '#bf3c00',
      800: '#993000',
      900: '#7a2600',
      950: '#421500',
    },
  },
  semantic: {
    primary: {
      50: '{brand.50}',
      100: '{brand.100}',
      200: '{brand.200}',
      300: '{brand.300}',
      400: '{brand.400}',
      500: '{brand.500}',
      600: '{brand.600}',
      700: '{brand.700}',
      800: '{brand.800}',
      900: '{brand.900}',
      950: '{brand.950}',
    },
    formField: {
      paddingX: '0.875rem',
      paddingY: '0.75rem',
    },
    colorScheme: {
      light: {
        surface: {
          0: '#ffffff',
          50: '#f4f5f6',
          100: '#eceef0',
          200: '#d7dadc',
          300: '#b9bdc1',
          400: '#8f949a',
          500: '#6b7075',
          600: '#50555a',
          700: '#3a3e42',
          800: '#26292b',
          900: '#1a1a1a',
          950: '#0d0d0d',
        },
        primary: {
          color: '{primary.500}',
          contrastColor: '#ffffff',
          hoverColor: '{primary.600}',
          activeColor: '{primary.700}',
        },
        text: {
          color: '{surface.900}',
          hoverColor: '{surface.950}',
          mutedColor: '{surface.500}',
          hoverMutedColor: '{surface.600}',
        },
      },
    },
  },
  components: {
    button: {
      root: {
        paddingX: '1.25rem',
        paddingY: '0.75rem',
        label: {
          fontWeight: '700',
        },
      },
    },
    card: {
      root: {
        borderRadius: '16px',
        shadow: '0 12px 40px 0 rgba(0, 0, 0, 0.1), 0 8px 32px 0 rgba(0, 0, 0, 0.08)',
      },
    },
  },
})

export default AppPreset
