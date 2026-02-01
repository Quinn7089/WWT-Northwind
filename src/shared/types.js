// Shared types and interfaces between frontend and backend

/**
 * @typedef {Object} ApiResponse
 * @property {*} [data] - Response data
 * @property {string} [message] - Response message
 * @property {boolean} success - Whether the request was successful
 * @property {string[]} [errors] - Array of error messages
 */

/**
 * @typedef {Object} TestMessage
 * @property {string} message - Test message content
 */

// Export for use in other modules
export const ApiResponse = {};
export const TestMessage = {};
export const WeatherForecast = {};
