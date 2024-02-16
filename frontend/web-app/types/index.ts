export type Auction = {
  reservePrice: number,
  seller: string,
  winner?: string,
  soldAmount: number,
  currentHighBid: number,
  createdAt: string,
  updatedAt: string,
  auctionEnd: string,
  status: string,
  make: string,
  model: string,
  year: number
  color: string,
  mileage: number
  imageUrl: string,
  id: string
};

export type AuctionUpdate = {
  make: string,
  model: string,
  year: number
  color: string,
  mileage: number
};

export type PagedResult<T> = {
  results: T[],
  totalCount: number,
  pageCount: number
};