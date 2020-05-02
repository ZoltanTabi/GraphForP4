export function Copy<T>(arg: T): T {
  return JSON.parse(JSON.stringify(arg)) as T;
}
